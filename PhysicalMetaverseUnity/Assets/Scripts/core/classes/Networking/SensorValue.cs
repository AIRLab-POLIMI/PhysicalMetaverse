
public class SensorValue
{

    #region Variables
 
    private OngoingValue _ongoingValue;
    private string _key;
        
    #endregion


    #region PARAMETERS

    public byte CurrentValue
    {
        get => _ongoingValue.CurrentVal;
    }

    #endregion
    
    
    #region Constructor
    
    public SensorValue(string key, float min, float max)
    {
        _key = key;
        _ongoingValue = new OngoingValue(min, max);
    }
        
    public SensorValue(string key, float min, float max, float tolerance)
    {
        _key = key;
        _ongoingValue = new OngoingValue(min, max, tolerance);
    }
    
    #endregion
    
    
    #region Methods

    public void OnNewValueReceived(float val) => _ongoingValue.SetCurrentValue(val);
        
    // checks if shouldSend is true and if it is returns a string "key:val" where val comes from SendCurrentValue;
    // otherwise returns empty string
    public string TryGetMsg() => _ongoingValue.ShouldSend() ? $"{_key}{Constants.KeyValDelimiter}{_ongoingValue.SendCurrentValue()}" : "";

    public void SetToleranceFromPercentageOfRange(float percentangeTolerance) =>
        _ongoingValue.SetToleranceFromPercentageOfRange(percentangeTolerance);
        
    #endregion
    
}