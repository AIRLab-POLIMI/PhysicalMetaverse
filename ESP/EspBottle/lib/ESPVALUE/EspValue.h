
#include <Arduino.h>


class EspValue {

    protected: 
        
        char * _key;

        float _curVal;
        float _lastSentVal;
        float _tolerance;

        bool _toSend = false;
        IPAddress _destinationIP;

    public:

        bool canSend() {
            return _toSend;
        }

        IPAddress getDestinationIp() {
            return _destinationIP;
        }

        EspValue(char* key, float tolerance) {
            _toSend = false;
            _key = key;
            _tolerance = tolerance;
            // Serial.print("[EspValue][constructor] - tolerance: ");
            // Serial.print(tolerance);
            // Serial.print(" - _tolerance: ");
            // Serial.println(tolerance);
        }

        bool keyEquals(char* tempKey) {
            return strcmp(tempKey, _key) == 0;;
        }

        char * getKey() {
            return _key;
        }

        void setConfig(bool send, IPAddress destinationIP) {
            _toSend = send;
            // actually, if send=false, the destinationIP is RESET
            _toSend 
                ? _destinationIP = destinationIP
                : _destinationIP = IPAddress();

            //Serial.print("[EspValue][setConfig] - to send: ");
            //Serial.println(_toSend);
        }

        String get_value() {

            // IF: 
            // - toSend is TRUE, and
            // - the last sent value is larger enough from the previous
            // return a String in the shape 'value', 
            // otherwise return empty String

            if (!_toSend) {
                return "";
            }

            float dif = abs(_curVal - _lastSentVal);

            if (dif > _tolerance) {
                String msg = String(_curVal);
                _lastSentVal = _curVal;
                return msg;
            } else
                return "";
        }

        String get_key_value() {

            // IF: 
            // - toSend is TRUE, and
            // - the last sent value is larger enough from the previous
            // return a String in the shape 'key:value', 
            // otherwise return empty String

            if (!_toSend) {
                // Serial.println("[EspValue][get_key_value] - NOT SEND");
                return "";
            }

            float dif = abs(_curVal - _lastSentVal);

            if (dif > _tolerance) {
                // Serial.print("[EspValue][get_key_value] - SEND - dif: ");
                // Serial.print(dif);
                // Serial.print(" - _tolerance: ");
                // Serial.print(_tolerance);
                // Serial.print(" - _curVal: ");
                // Serial.print(_curVal);
                // Serial.print(" - _last sent val: ");
                // Serial.println(_lastSentVal);

                String msg = String(_key);
                msg += ':';
                msg += _curVal;
                _lastSentVal = _curVal;
                return msg;
            } else {
                // Serial.print("[EspValue][get_key_value] - NOT SEND - dif: ");
                // Serial.print(dif);
                // Serial.print(" - _tolerance: ");
                // Serial.println(_tolerance);

                return "";
            }
        }

        void onNewValueReceived(float newVal) {
            // Serial.print("[EspValue][onNewValueReceived]: key: ");
            // Serial.print(_key);
            // Serial.print("New val: ");
            // Serial.println(newVal);
            // Serial.println("");
            _curVal = newVal;
        }
};