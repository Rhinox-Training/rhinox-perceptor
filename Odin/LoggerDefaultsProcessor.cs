#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Rhinox.Perceptor.Odin
{
    public class LoggerDefaultsProcessor : OdinAttributeProcessor<LoggerDefaults>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            base.ProcessChildMemberAttributes(parentProperty, member, attributes);

            if (member.Name == nameof(LoggerDefaults.Settings))
            {
                attributes.Add(new ListDrawerSettingsAttribute()
                {
                    HideAddButton = true,
                    HideRemoveButton = true,
                    Expanded = true,
                    DraggableItems = false
                });
            }
        }
    }
}
#endif