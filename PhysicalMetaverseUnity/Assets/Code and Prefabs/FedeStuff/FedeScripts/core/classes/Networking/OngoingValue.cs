
using UnityEngine;


public class OngoingValue
{
    
    #region Variables
    
        private readonly float DEFAULT_TOLERANCE_RANGE_PERCENTAGE = 2;
        
        private readonly byte DEFAULT_CENTER_VALUE = 127;

        private float _tolerance;
        private readonly float _min;
        private readonly float _max;
        private readonly float _range;

        private readonly float _deadZone;

    #endregion

    
    #region Properties
    
        public byte CurrentVal
        {
            get; private set;
        }

        private byte LastSentVal
        {
            get; set;
        }
        
    #endregion
    
    
    #region Constructor
    
        public OngoingValue(float min, float max, float deadzone = 0)
        {
            _min = min;
            _range = max - min;
            _deadZone = deadzone;
            SetToleranceFromPercentageOfRange(DEFAULT_TOLERANCE_RANGE_PERCENTAGE); // use default tolerance percentage if not specified
        }
        
        //public OngoingValue(float min, float max, float tolerance)
        //{
        //    _min = min;
        //    _range = max - min;
        //    _tolerance = tolerance;
        //}

    #endregion
    
    
    #region Methods

        public void SetCurrentValue(float val)
        {
            CurrentVal = MapToByte(val);
            //if (CurrentVal < _deadZone)
            //    CurrentVal = 0;
//
            //if (CurrentVal > 255 -_deadZone)
            //    CurrentVal = 255;

            if (Mathf.Abs(CurrentVal - DEFAULT_CENTER_VALUE) < _deadZone)
                CurrentVal = DEFAULT_CENTER_VALUE;
        }
        
        public bool ShouldSend()
        {
            if (Mathf.Abs(CurrentVal - LastSentVal) > _tolerance)
            {
                LastSentVal = CurrentVal;
                return true;
            }
            // send if value is min or max and last sent value is not exactly the same 
            if ((CurrentVal == 0 || CurrentVal == 255 || CurrentVal == DEFAULT_CENTER_VALUE) && CurrentVal != LastSentVal)
            {
                LastSentVal = CurrentVal;
                return true;
            }
            return false;
        }

        public byte SendCurrentValue()
        {
            LastSentVal = CurrentVal;
            return CurrentVal;
        }
           
        // function that sets tolerance as a percentage of the range
        public void SetToleranceFromPercentageOfRange(float percentage) => 
            _tolerance = _range * percentage / 100;
        
        // function that maps the range min, max into the range 0, 255
        private byte MapToByte(float val) =>
            (byte) Mathf.Clamp((val - _min) * 255 / (_range), 0, 255);
        
    #endregion
}
