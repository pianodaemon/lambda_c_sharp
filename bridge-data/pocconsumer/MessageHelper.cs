namespace POCConsumer;

using System;
using Newtonsoft.Json.Linq;

public static class MessageHelper
{
    public const string FileKeyField = "fileKey";
    public const string TargetPathField = "targetPath";

    public static BridgePartialData DecodeMessage(string messageBody)
    {
        JObject json = JObject.Parse(messageBody);
        return new BridgePartialData(
            json[FileKeyField]?.ToString() ?? throw new ArgumentNullException(FileKeyField, $"{FileKeyField} is required and cannot be null or empty."),
            json[TargetPathField]?.ToString() ?? throw new ArgumentNullException(TargetPathField, $"{TargetPathField} is required and cannot be null or empty.")
        );
    }
}
