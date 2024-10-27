using Database.Enums.Verification;

namespace Database.Utilities;

public static class EnumUtils
{
    /// <summary>
    /// Marks a <see cref="VerificationStatus"/> as Rejected if the status is PreRejected
    /// and Verified if the status is PreVerified
    /// </summary>
    /// <param name="status">The 'pre' verification status to confirm</param>
    /// <returns>
    /// The confirmed status, or the provided status if
    /// <see cref="status"/> is not PreRejected or PreVerified
    /// </returns>
    public static VerificationStatus ConfirmPreStatus(VerificationStatus status) =>
        status switch
        {
            VerificationStatus.PreRejected => VerificationStatus.Rejected,
            VerificationStatus.PreVerified => VerificationStatus.Verified,
            _ => status
        };
}
