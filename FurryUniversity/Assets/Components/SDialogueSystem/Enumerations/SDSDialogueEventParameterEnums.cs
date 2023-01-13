namespace SDS.Enumerations
{
    /// <summary>
    /// <see cref="SDSDialogueEventType.ImageOperations"/> 的子事件类型，真是一套又一套嗷
    /// </summary>
    public enum SDSDialogueImageEventOperations
    {
        Show = 0,
        Hide,
        Move
    }

    /// <summary>
    /// <see cref="SDSDialogueEventType.BackgroundImageOperations"/> 的子事件类型
    /// </summary>
    public enum SDSDialogueBackgroundImageEventOperations
    {
        Show = 0,
        Hide,
    }

    public enum SDSSpritePresetPosition
    {
        CustomizedPosition,
        Left,
        Middle,
        Right,
    }

    /// <summary>
    /// <see cref="SDSDialogueEventType.BGMOperations"/> 的子事件类型
    /// </summary>
    public enum SDSDialogueBGMEventOperations
    {
        Play = 0,
        Pause,
        Resume,
        Stop,
    }

    /// <summary>
    /// <see cref="SDSDialogueEventType.SFXOperations"/> 的子事件类型
    /// </summary>
    public enum SDSDialogueSFXEventOperations
    {
        //SFX有哪些子事件，小编也不知道捏
        Play = 0,
    }
}