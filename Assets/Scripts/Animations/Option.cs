using System;

public abstract class Option
{
    public Type type;
    public string tag;
    public string info;
    
    public static Option<T> Create<T>(string tag, WiiGetter<T> getter, WiiSetter<T> setter)
    {
        return Create(tag, null, getter, setter);
    }
    
    public static Option<T> Create<T>(string tagId, string infoId, WiiGetter<T> getter, WiiSetter<T> setter)
    {
        string tag = TextController.GetTranslation(tagId);
        string info = TextController.GetTranslation(infoId);
        Option<T> option = new Option<T>(tag);
        option.getter = getter;
        option.setter = setter;
        option.type = typeof(T);
        option.info = info;
        return option;
    }
}

public class Option<T> : Option
{
    public WiiGetter<T> getter;
    public WiiSetter<T> setter;
    
    public Option(string tag)
    {
        this.tag = tag;
    }
}