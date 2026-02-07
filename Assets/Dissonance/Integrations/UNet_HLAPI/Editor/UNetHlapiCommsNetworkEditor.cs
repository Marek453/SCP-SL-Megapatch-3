using Dissonance.Editor;
using UnityEditor;
using UnityEngine;

namespace Dissonance.Integrations.UNet_HLAPI.Editor
{
    [CustomEditor(typeof(HlapiCommsNetwork))]
    public class UNetCommsNetworkEditor
        : BaseDissonnanceCommsNetworkEditor<HlapiCommsNetwork, HlapiServer, HlapiClient, HlapiConn, Unit, Unit>
    {
        private bool _advanced;

        private SerializedProperty _typeCodeProperty;
        private SerializedProperty _reliableChannelProperty;
        private SerializedProperty _unreliableChannelProperty;

        protected void OnEnable()
        {
            _typeCodeProperty = serializedObject.FindProperty("TypeCode");
            _reliableChannelProperty = serializedObject.FindProperty("ReliableSequencedChannel");
            _unreliableChannelProperty = serializedObject.FindProperty("UnreliableChannel");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {

                //Set the two QoS channels
                EditorGUILayout.HelpBox("Dissonance requires 2 HLAPI QoS channels.", MessageType.Info);

                EditorGUILayout.PropertyField(_reliableChannelProperty);
                EditorGUILayout.PropertyField(_unreliableChannelProperty);

                var uc = _unreliableChannelProperty.intValue;
                var rc = _reliableChannelProperty.intValue;

                if (uc < 0 || uc >= byte.MaxValue || rc < 0 || rc >= byte.MaxValue)
                    EditorGUILayout.HelpBox("Channel IDs must be between 0 and 255", MessageType.Error);
                else if (uc == rc)
                    EditorGUILayout.HelpBox("Channel IDs must be unique", MessageType.Error);

                _advanced = EditorGUILayout.Foldout(_advanced, "Advanced Configuration");
                if (_advanced)
                {
                    //Set type code
                    EditorGUILayout.HelpBox("Dissonance requires a HLAPI type code. If you are not sending raw HLAPI network packets you should use the default value.", MessageType.Info);
                    EditorGUILayout.PropertyField(_typeCodeProperty);

                    var tc = _typeCodeProperty.intValue;

                    if (tc >= ushort.MaxValue || tc < 1000)
                        EditorGUILayout.HelpBox("Event code must be between 1000 and 65535", MessageType.Error);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}