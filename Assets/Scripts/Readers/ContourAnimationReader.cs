using System;
using UnityEngine;

public class ContourAnimationReader : ContourReader
{
    public override Type BuilderType => typeof(ContourAnimationBuilder);
    public override string BuilderName => "Animation Builder";
}
