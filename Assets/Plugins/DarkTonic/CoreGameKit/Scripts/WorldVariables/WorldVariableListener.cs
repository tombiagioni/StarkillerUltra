#if UNITY_4_6 || UNITY_5_0
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
[AddComponentMenu("Dark Tonic/Killer Waves/Listeners/World Variable Listener")]
// ReSharper disable once CheckNamespace
public class WorldVariableListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
	public string variableName = "";
	public WorldVariableTracker.VariableType vType = WorldVariableTracker.VariableType._integer;
	public bool displayVariableName = false;
	public int decimalPlaces = 1;
	public bool useCommaFormatting = true;
    // ReSharper restore InconsistentNaming
	
	private int _variableValue;   
	private float _variableFloatValue;
	
	private Text _text;
	
    // ReSharper disable once UnusedMember.Local
	void Awake() {
		_text = GetComponent<Text>();
	}
	
	public virtual void UpdateValue(int newValue) {
		_variableValue = newValue;
		var valFormatted = string.Format("{0}{1}",
		                                 
		                                 displayVariableName ? variableName + ": " : "",
		                                 _variableValue.ToString("N0"));
		
		if (!useCommaFormatting) {
			valFormatted = valFormatted.Replace(",", "");
		}
		
		if (_text == null || !SpawnUtility.IsActive(_text.gameObject)) {
			return;
		}
		
		_text.text = valFormatted;
	}
	
	public virtual void UpdateFloatValue(float newValue) {
		_variableFloatValue = newValue;
		var valFormatted = string.Format("{0}{1}",
		                                 displayVariableName ? variableName + ": " : "",
		                                 _variableFloatValue.ToString("N" + decimalPlaces));
		
		if (!useCommaFormatting) {
			valFormatted = valFormatted.Replace(",", "");
		}
		
		_text.text = valFormatted;
	}
}
#else
using UnityEngine;

[AddComponentMenu("Dark Tonic/Core GameKit/Listeners/World Variable Listener")]
// ReSharper disable once CheckNamespace
public class WorldVariableListener : MonoBehaviour {
    // ReSharper disable InconsistentNaming
	public string variableName = "";
	public WorldVariableTracker.VariableType vType = WorldVariableTracker.VariableType._integer;
	public int decimalPlaces = 1;
	public bool useCommaFormatting = true;
    public int xStart = 50; // ALSO delete this when you get rid of the OnGUI section. You won't need it.
    // ReSharper restore InconsistentNaming
	
	private int _variableValue;
	private float _variableFloatValue;
	
    // ReSharper disable once UnusedMember.Local
	void Reset() {
		var src = GetComponent<WorldVariable>();
	    if (src == null)
	    {
	        return;
	    }
	    src.listenerPrefab = this;
	    variableName = name;
	}
	
	public virtual void UpdateValue(int newValue) {
		_variableValue = newValue;
	}
	
	public virtual void UpdateFloatValue(float newValue) {
		_variableFloatValue = newValue;
	}
	
	// This is just used for illustrative purposes. You might replace this with code to update a non-Unity GUI text element. If you use NGUI, please install the optional package "NGUI_CoreGameKit" to get an NGUI version of this script, replacing this one.
    // ReSharper disable once UnusedMember.Local
	void OnGUI() {
		string valFormatted;
		switch (vType) {
		case WorldVariableTracker.VariableType._integer:
			valFormatted = _variableValue.ToString("N0");
			if (!useCommaFormatting) {
				valFormatted = valFormatted.Replace(",", "");
			}
			GUI.Label(new Rect(xStart, 120, 180, 40), variableName + ": " + valFormatted);
			break;
		case WorldVariableTracker.VariableType._float:
			valFormatted = _variableFloatValue.ToString("N" + decimalPlaces);
			if (!useCommaFormatting) {
				valFormatted = valFormatted.Replace(",", "");
			}
			GUI.Label(new Rect(xStart, 120, 180, 40), variableName + ": " + valFormatted);
			break;
		default:
			LevelSettings.LogIfNew("Add code for varType: " + vType.ToString());
			break;
		}
	}
}
#endif