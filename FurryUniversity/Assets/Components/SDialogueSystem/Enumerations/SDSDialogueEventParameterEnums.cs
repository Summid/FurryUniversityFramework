namespace SDS.Enumerations
{
    /// <summary>
    /// <see cref="SDSDialogueEventType.ImageOperations"/> 的子事件类型，真是一套又一套嗷
    /// </summary>
    public enum SDSDialogueImageEventOperations
    {
        Show,
        Hide,
        Move
    }

    /// <summary>
    /// <see cref="SDSDialogueEventType.BackgroundImageOperations"/> 的子事件类型
    /// </summary>
    public enum SDSDialogueBackgroundImageEventOperations
    {
        Show,
        Hide,
    }

    public enum SDSSpritePresetPosition
    {
        CustomizedPosition,
        Left,
        Middle,
        Right,
    }
}