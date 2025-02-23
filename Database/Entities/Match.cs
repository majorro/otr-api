﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Database.Entities.Interfaces;
using Database.Entities.Processor;
using Database.Enums.Verification;
using Database.Utilities;

namespace Database.Entities;

/// <summary>
/// A match played in a <see cref="Tournament"/>
/// </summary>
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.CircularDependency")]
public class Match : UpdateableEntityBase, IProcessableEntity, IAdminNotableEntity<MatchAdminNote>,
    IAuditableEntity<MatchAudit>
{
    /// <summary>
    /// osu! id
    /// </summary>
    /// <example>https://osu.ppy.sh/community/matches/[113475484]</example>
    public long OsuId { get; set; }

    /// <summary>
    /// Name of the lobby the match was played in
    /// </summary>
    /// <example>5WC2024: (France) vs (Germany)</example>
    [MaxLength(512)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp for the beginning of the match
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Timestamp for the end of the match
    /// </summary>
    public DateTime EndTime { get; set; }

    public VerificationStatus VerificationStatus { get; set; }

    [AuditIgnore]
    public DateTime LastProcessingDate { get; set; }

    /// <summary>
    /// Rejection reason
    /// </summary>
    public MatchRejectionReason RejectionReason { get; set; }

    /// <summary>
    /// Warning flags
    /// </summary>
    public MatchWarningFlags WarningFlags { get; set; }

    /// <summary>
    /// Processing status
    /// </summary>
    public MatchProcessingStatus ProcessingStatus { get; set; }

    /// <summary>
    /// Id of the <see cref="Entities.Tournament"/> the match was played in
    /// </summary>
    public int TournamentId { get; set; }

    /// <summary>
    /// The <see cref="Entities.Tournament"/> the match was played in
    /// </summary>
    public Tournament Tournament { get; set; } = null!;

    /// <summary>
    /// Id of the <see cref="User"/> that submitted the match
    /// </summary>
    public int? SubmittedByUserId { get; set; }

    /// <summary>
    /// The <see cref="User"/> that submitted the match
    /// </summary>
    public User? SubmittedByUser { get; set; }

    /// <summary>
    /// Id of the <see cref="User"/> that verified the match
    /// </summary>
    public int? VerifiedByUserId { get; set; }

    /// <summary>
    /// The <see cref="User"/> that verified the match
    /// </summary>
    public User? VerifiedByUser { get; set; }

    /// <summary>
    /// The <see cref="MatchRoster"/>
    /// </summary>
    public ICollection<MatchRoster> Rosters { get; set; } = [];

    /// <summary>
    /// A collection of the <see cref="Game"/>s played in the match
    /// </summary>
    public ICollection<Game> Games { get; set; } = [];

    /// <summary>
    /// A collection of <see cref="Entities.PlayerMatchStats"/>, one for each <see cref="Player"/> that participated
    /// </summary>
    public ICollection<PlayerMatchStats> PlayerMatchStats { get; set; } = [];

    /// <summary>
    /// A collection of <see cref="RatingAdjustment"/>s, one for each <see cref="Player"/> that participated
    /// </summary>
    public ICollection<RatingAdjustment> PlayerRatingAdjustments { get; set; } = [];

    public ICollection<MatchAudit> Audits { get; set; } = [];

    public ICollection<MatchAdminNote> AdminNotes { get; set; } = [];

    [NotMapped] public int? ActionBlamedOnUserId { get; set; }

    public void ResetAutomationStatuses(bool force)
    {
        var matchUpdate = force || (VerificationStatus != VerificationStatus.Rejected &&
                                    VerificationStatus != VerificationStatus.Verified);

        if (!matchUpdate)
        {
            return;
        }

        VerificationStatus = VerificationStatus.None;
        WarningFlags = MatchWarningFlags.None;
        RejectionReason = MatchRejectionReason.None;
        ProcessingStatus = MatchProcessingStatus.NeedsAutomationChecks;
    }

    public void ConfirmPreVerificationStatus() => VerificationStatus = EnumUtils.ConfirmPreStatus(VerificationStatus);
}
