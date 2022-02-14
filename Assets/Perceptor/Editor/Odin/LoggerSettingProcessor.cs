#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Rhinox.Perceptor.Odin
{
    internal class LoggerSettingProcessor : OdinAttributeProcessor<LoggerSetting>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);

            if (member.Name == nameof(LoggerSetting.TypeName))
            {
                attributes.Add(new HorizontalGroupAttribute("Entry"));
                attributes.Add(new HideLabelAttribute());
                attributes.Add(new ShowInInspectorAttribute());
                attributes.Add(new ReadOnlyAttribute());
            }
            else if (member.Name == nameof(LoggerSetting.FullTypeName))
            {
                attributes.Add(new HideInInspector());
            }
            else if (member.Name == nameof(LoggerSetting.Level))
            {
                attributes.Add(new HorizontalGroupAttribute("Entry", width: 0.3f));
                attributes.Add(new HideLabelAttribute());
            }
            else if (member.Name == nameof(LoggerSetting.ThrowExceptionOnFatal))
            {
                attributes.Add(new HorizontalGroupAttribute("Entry", width: 0.2f));
                attributes.Add(new LabelTextAttribute("Throws"));
            }
        }
    }
}
#endif