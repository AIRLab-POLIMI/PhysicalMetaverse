

#ifndef Included_NameModel_H

#define Included_NameModel_H


#include <Arduino.h>
#include <NewPing.h>


// WRAPPER CLASS for NEWPING object


#define DEFAULT_PING_SPEED 40
#define DEFAULT_MAX_DISTANCE 450

#define DEFAULT_INVALID_VALUE 0

#define DEFAULT_MAX_CONSECUTIVE_INVALID 6
#define DEFAULT_MAX_ELAPSED_TIME 100

#define DEFAULT_TOLERANCE 2




class NewSonar
{
    private:
        NewPing _sonar;

        char * _id;

        unsigned long _maxDistance;

        unsigned long _lastSentVal;
        unsigned long _curVal;

        unsigned long _lastTimeRecv;

        unsigned long _curConsecutiveInvalid;

    public:

        NewSonar(uint8_t trigPin, uint8_t echoPin, char* id, unsigned int maxDistance = DEFAULT_MAX_DISTANCE) : _sonar(trigPin, echoPin, maxDistance) {
            
            _maxDistance = maxDistance;

            _id = id;

            _lastSentVal = -1000;
            _curVal = DEFAULT_INVALID_VALUE;

            _lastTimeRecv = millis();
            _curConsecutiveInvalid = 0;
        }
        
        String get_key_value_serial() {

            // check if the last sent value is larger enough from the previous
            // if yes, write and return TRUE, otherwise write nothing and return false
        
            unsigned long dif = _lastSentVal > _curVal 
                ? _lastSentVal - _curVal
                : _curVal - _lastSentVal;

            if (dif > DEFAULT_TOLERANCE) {
                String msg = String(_id);
                msg += ':';
                msg += _curVal;
                _lastSentVal = _curVal;
                return msg;
            } else {
                return "";
            }
        }

        void timer_stop() {
            _sonar.timer_stop();
        }

         bool check_timer() {
            return _sonar.check_timer();
        }

        void ping_timer(void (*userFunc)(), unsigned int max_cm_distance = 0U) {
            _sonar.ping_timer(userFunc);
        }

        unsigned long get_ping_result() {
            return _sonar.ping_result;
        }

        void onInvalidRecv() {
            _curConsecutiveInvalid += 1;

            if (_curConsecutiveInvalid > DEFAULT_MAX_CONSECUTIVE_INVALID && _curVal != DEFAULT_INVALID_VALUE) {
                _curVal = DEFAULT_INVALID_VALUE;
            }
        }

        void checkTimeout() {
            unsigned long now = millis();

            if (now - _lastTimeRecv > DEFAULT_MAX_ELAPSED_TIME) 
                onInvalidRecv();
        }

        void onNewValueReceived() {

            _curVal = _sonar.ping_result / US_ROUNDTRIP_CM;
            if (_curVal > _maxDistance) {
                onInvalidRecv();
                return;
            }

            _lastTimeRecv = millis();
            _curConsecutiveInvalid = 0;
        }
};

#endif
