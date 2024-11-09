using System.Reflection;
using System.Xml.XPath;
using JetBrains.Annotations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.SwaggerGen.Filters;

/// <summary>
/// Enriches the metadata of <see cref="OpenApiSchema"/>s for enums.
/// Adds an extension denoting the enum as being a bitwise flag or not.
/// Adds an extension containing a list of all enum member names.
/// Adds an extension containing a list of descriptions for all enum members
/// </summary>
/// <param name="xmlDocPaths">A list of paths to xml documents used to extract documentation</param>
[UsedImplicitly]
public class EnumMetadataSchemaFilter(string[] xmlDocPaths) : ISchemaFilter
{
    private readonly IList<XPathNavigator> _xmlNavigators = xmlDocPaths
        .Where(File.Exists)
        .Select(path => new XPathDocument(path).CreateNavigator())
        .ToList();

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        Type? type = context.Type;
        if (!type.IsEnum)
        {
            return;
        }

        // If this is not populated in the schema that means it is a reference schema,
        // and we only want to include the extensions on the enum's "concrete" schema
        if (!schema.Enum.Any())
        {
            return;
        }

        // Add the bitwise flag extension
        if (type.GetCustomAttribute<FlagsAttribute>() is not null)
        {
            schema.Extensions.Add(ExtensionKeys.EnumBitwiseFlag, new OpenApiBoolean(true));
        }

        // Add the enum names extension
        if (!schema.Extensions.ContainsKey(ExtensionKeys.EnumNames))
        {
            var namesExtensionValue = new OpenApiArray();
            namesExtensionValue.AddRange(type.GetEnumNames().Select(n => new OpenApiString(n)));
            schema.Extensions.Add(ExtensionKeys.EnumNames, namesExtensionValue);
        }

        // Add the enum descriptions extension
        if (schema.Extensions.ContainsKey(ExtensionKeys.EnumDescriptions))
        {
            return;
        }

        // Get the expected xml node paths for each enum member
        var enumNodePaths = type
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Cast<MemberInfo>()
            .Select(XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty)
            .ToList();

        var descriptionsExtensionValue = new OpenApiArray();
        foreach (var nodePath in enumNodePaths)
        {
            // Try to get the xml node from any of the given navigators
            XPathNavigator? memberNode = _xmlNavigators
                .Select(nav => nav.SelectSingleNode($"/doc/members/member[@name='{nodePath}']"))
                .OfType<XPathNavigator>()
                .FirstOrDefault();

            var output = "";

            if (memberNode is null)
            {
                descriptionsExtensionValue.Add(new OpenApiString(output));
                continue;
            }

            // Try to get the xml summary
            XPathNavigator? summaryNode = memberNode.SelectSingleNode("summary");
            if (summaryNode is not null)
            {
                output += XmlCommentsTextHelper.Humanize(summaryNode.InnerXml);
            }

            // Try to get the xml remarks
            XPathNavigator? remarksNode = memberNode.SelectSingleNode("remarks");
            if (remarksNode is not null)
            {
                output += string.IsNullOrEmpty(output)
                    ? XmlCommentsTextHelper.Humanize(remarksNode.InnerXml)
                    : "\n\n" + XmlCommentsTextHelper.Humanize(remarksNode.InnerXml);
            }

            descriptionsExtensionValue.Add(new OpenApiString(output));
        }

        schema.Extensions.Add(ExtensionKeys.EnumDescriptions, descriptionsExtensionValue);
    }
}
