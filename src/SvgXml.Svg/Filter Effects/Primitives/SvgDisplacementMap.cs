﻿using System;
using Xml;

namespace Svg.FilterEffects
{
    [Element("feDisplacementMap")]
    public class SvgDisplacementMap : SvgFilterPrimitive,
        ISvgCommonAttributes,
        ISvgPresentationAttributes,
        ISvgStylableAttributes
    {
        [Attribute("in", SvgNamespace)]
        public string? Input
        {
            get => this.GetAttribute("in", false, null);
            set => this.SetAttribute("in", value);
        }

        [Attribute("in2", SvgNamespace)]
        public string? Input2
        {
            get => this.GetAttribute("in2", false, null);
            set => this.SetAttribute("in2", value);
        }

        [Attribute("scale", SvgNamespace)]
        public string? Scale
        {
            get => this.GetAttribute("scale", false, "0");
            set => this.SetAttribute("scale", value);
        }

        [Attribute("xChannelSelector", SvgNamespace)]
        public string? XChannelSelector
        {
            get => this.GetAttribute("xChannelSelector", false, "A");
            set => this.SetAttribute("xChannelSelector", value);
        }

        [Attribute("yChannelSelector", SvgNamespace)]
        public string? YChannelSelector
        {
            get => this.GetAttribute("yChannelSelector", false, "A");
            set => this.SetAttribute("yChannelSelector", value);
        }

        public override void SetPropertyValue(string key, string? value)
        {
            base.SetPropertyValue(key, value);
            switch (key)
            {
                case "in":
                    Input = value;
                    break;
                case "in2":
                    Input2 = value;
                    break;
                case "scale":
                    Scale = value;
                    break;
                case "xChannelSelector":
                    XChannelSelector = value;
                    break;
                case "yChannelSelector":
                    YChannelSelector = value;
                    break;
            }
        }

        public override void Print(Action<string> write, string indent)
        {
            base.Print(write, indent);

            if (Input != null)
            {
                write($"{indent}{nameof(Input)}: \"{Input}\"");
            }
            if (Input2 != null)
            {
                write($"{indent}{nameof(Input2)}: \"{Input2}\"");
            }
            if (Scale != null)
            {
                write($"{indent}{nameof(Scale)}: \"{Scale}\"");
            }
            if (XChannelSelector != null)
            {
                write($"{indent}{nameof(XChannelSelector)}: \"{XChannelSelector}\"");
            }
            if (YChannelSelector != null)
            {
                write($"{indent}{nameof(YChannelSelector)}: \"{YChannelSelector}\"");
            }
        }
    }
}
