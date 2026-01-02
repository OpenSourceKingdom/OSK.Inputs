using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Services;

internal partial class InputProcessor
{
    #region Logging

    [LoggerMessage(eventId: 1, LogLevel.Warning, "An input was received for a device named '{deviceIdentity}' but no user was found to be paired to it, thus the input will be ignored.")]
    private static partial void LogNoInputUserForDeviceWarning(ILogger logger, InputDeviceIdentity deviceIdentity);

    [LoggerMessage(eventId: 3, LogLevel.Warning, "An attempt was made to use an active scheme with input definition '{desiredDefinitionName}' but it did not exist, defaulting to '{defaultDefinitionName}'.")]
    private static partial void LogInvalidDefinitionUsageWarning(ILogger logger, string desiredDefinitionName, string defaultDefinitionName);

    [LoggerMessage(eventId: 4, LogLevel.Warning, "An attempt was made to use an active scheme '{desiredSchemeName}' with input definition '{definitionName}' but the scheme did not exist, defaulting to '{defaultSchemeName}'.")]
    private static partial void LogInvalidSchemeUsageWarning(ILogger logger, string definitionName, string desiredSchemeName, string defaultSchemeName);

    [LoggerMessage(eventId: 5, LogLevel.Debug, "User {userId} has changed their scheme to use a scheme '{schemeName}' for input definition '{definitionName}'.")]
    private static partial void LogSchemeChangeDebug(ILogger logger, int userId, string definitionName, string schemeName);

    [LoggerMessage(eventId: 6, LogLevel.Debug, "User {userId} has joined the session, creating new input tracker.")]
    private static partial void LogUserJoinedDebug(ILogger logger, int userId);

    [LoggerMessage(eventId: 7, LogLevel.Debug, "User {userId} has been removed from the session, removing input tracker.")]
    private static partial void LogUserRemovedDebug(ILogger logger, int userId);

    [LoggerMessage(eventId: 8, LogLevel.Information, "A user notification, of type '{userEventTypeName}', was sent that was not supported.")]
    private static partial void LogUnknownUserNotificationInformation(ILogger logger, string userEventTypeName);

    [LoggerMessage(eventId: 9, LogLevel.Information, "An input device, '{deviceIdentifier}', sent input but was not paird due to the policy's manual device handling")]
    private static partial void LogUnpairdDeviceDueToPolicyInformation(ILogger logger, RuntimeDeviceIdentifier deviceIdentifier);

    [LoggerMessage(eventId: 10, LogLevel.Information, "An input device, '{deviceIdentifier}, sent input but was not paired because it was not part of a support device combination.")]
    private static partial void LogUnpairedDeviceDueToUnsupportedCombinationInformation(ILogger logger, RuntimeDeviceIdentifier deviceIdentifier);

    [LoggerMessage(eventId: 11, LogLevel.Debug, "A new input device, '{deviceIdentifier}', sent input and no user was found to possess it, new user being created due to policy's settings.")]
    private static partial void LogNewUserCreateDebug(ILogger logger, RuntimeDeviceIdentifier deviceIdentifier);

    [LoggerMessage(eventId: 12, LogLevel.Debug, "An input device, '{deviceIdentifier}', sent input for user {userId} but user input has not been received yet, creating new input tracker.")]
    private static partial void LogNewInputTrackerForUnregisteredUserDebug(ILogger logger, int userId, RuntimeDeviceIdentifier deviceIdentifier);

    [LoggerMessage(eventId: 13, LogLevel.Debug, "Input processing pause state changed, tracking input: {pause}")]
    private static partial void LogTogglePauseDebug(ILogger logger, bool pause);

    [LoggerMessage(eventId: 14, LogLevel.Information, "Input was received for input type '{inputTypeName}' but it is not a supported input type, ignoring int processing.")]
    private static partial void LogUnsupportedInputTypeInformation(ILogger logger, string inputTypeName);

    [LoggerMessage(eventId: 15, LogLevel.Debug, "Input received, from device {deviceIdentifier}, for user {userId} has triggered an action '{actionName}' for input scheme {activeScheme}.")]
    private static partial void LogInputActionTriggeredDebug(ILogger logger, int userId, RuntimeDeviceIdentifier deviceIdentifier, ActiveInputScheme activeScheme, string actionName);

    [LoggerMessage(eventId: 16, LogLevel.Warning, "An attempt was made to pair device {deviceIdentifier} to user {userId} but it failed.")]
    public static partial void LogDevicePairingFailedWarning(ILogger logger, int userId, RuntimeDeviceIdentifier deviceIdentifier);

    #endregion
}
