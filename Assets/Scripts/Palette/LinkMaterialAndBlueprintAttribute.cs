using UnityEngine;
using System;

[Serializable]
public class LinkMaterialAndBlueprintAttribute : Attribute
{
    public string valueField;
    public string modeField;

    public LinkMaterialAndBlueprintAttribute(string valueFieldName, string modeFieldName)
    {
        valueField = valueFieldName;
        modeField = modeFieldName;
    }
}
