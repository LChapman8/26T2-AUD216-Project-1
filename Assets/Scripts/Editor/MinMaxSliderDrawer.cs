using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Vector2)
        {
            EditorGUI.LabelField(position, label.text, "Use MinMaxSlider with Vector2.");
            return;
        }

        var range = attribute as MinMaxSliderAttribute;
        var rangeValue = property.vector2Value;

        // Adding a prefix label to display the property name
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Create a Rect for the slider
        var sliderPosition = new Rect(position.x, position.y, position.width - 90, position.height);
        // Create Rects for the min and max values
        var minValPosition = new Rect(position.x + position.width - 90, position.y, 40, position.height);
        var maxValPosition = new Rect(position.x + position.width - 45, position.y, 40, position.height);

        EditorGUI.BeginChangeCheck();
        EditorGUI.MinMaxSlider(sliderPosition, ref rangeValue.x, ref rangeValue.y, range.min, range.max);

        rangeValue.x = EditorGUI.FloatField(minValPosition, rangeValue.x);
        rangeValue.y = EditorGUI.FloatField(maxValPosition, rangeValue.y);

        if (EditorGUI.EndChangeCheck())
        {
            property.vector2Value = rangeValue;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + 16;
    }
}
