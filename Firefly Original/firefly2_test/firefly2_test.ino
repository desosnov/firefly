#include "FastLED.h"

// APA102 LED strip constants
#define DATA_PIN 22
#define CLOCK_PIN 3

// WS2812 LED strip constants
#define DATA_PIN_LEFT 3
#define DATA_PIN_RIGHT 5

// SHARED
#define NUM_LEDS_LEFT 1440
#define NUM_LEDS_RIGHT 1440

CRGB leds_left[NUM_LEDS_LEFT];
CRGB leds_right[NUM_LEDS_RIGHT];

// LED brightness constants
#define MAX_BRIGHTNESS 255
#define AMPS_PER_STRIP_TARGET 7500.0
#define AMPS_PER_STRIP_MAX 10000.0
#define BRIGHTNESS_CHANGE_THRESHOLD 0.02
#define BRIGHTNESS_CHANGE_SPEED 0.1

double curBrightness = 0.0;

// Universal animation constants
#define DURATION_MIN 10000
#define DURATION_RANGE 10000
#define SATURATION_MIN 100


// Worm animation constants
#define WORM_SIZE 20

// Random worms animation constants
#define RANDOM_WORMS_SPEED_MIN 1
#define RANDOM_WORMS_SPEED_MAX 10
#define RANDOM_WORMS_MIN_SIZE 3
#define RANDOM_WORMS_MAX_SIZE 50

// Takeover and fade animation constants
#define TAKEOVER_FADE_INTERVAL_MIN 2000
#define TAKEOVER_FADE_INTERVAL_RANGE 6000

// Takeover and stay animation constants
#define TAKEOVER_STAY_INTERVAL_MIN 2000
#define TAKEOVER_STAY_INTERVAL_RANGE 6000
#define TAKEOVER_STAY_MIN_HUE_DIST 40
#define TAKEOVER_STAY_MIN_VALUE_THRESHOLD 0.9 




// UTILITIES

int hueDist(int hue1, int hue2) {
  int lower = min(hue1, hue2);
  int higher = max(hue1, hue2);
  if(higher - lower > 128) {
    lower += 256;
  }

  return abs(higher-lower);
}

void clearLeds() {
  fill_solid(leds_left, NUM_LEDS_LEFT, CRGB::Black);
  fill_solid(leds_right, NUM_LEDS_RIGHT, CRGB::Black);
}

void setAutoBrightness() {
  double powerDrawLeft = 0.0, powerDrawRight = 0.0;
  for(int i = 0; i < NUM_LEDS_LEFT; i++) {
    powerDrawLeft += (double)(leds_left[i].red + leds_left[i].green + leds_left[i].blue) / 255.0 * 20.0;
  }
  for(int i = 0; i < NUM_LEDS_RIGHT; i++) {
    powerDrawRight += (double)(leds_right[i].red + leds_right[i].green + leds_right[i].blue) / 255.0 * 20.0;
  }
  double powerDraw = powerDrawLeft;

  if(powerDraw > 0.0) {
    double desiredBrightness = AMPS_PER_STRIP_TARGET / powerDraw;
    if(abs(curBrightness - desiredBrightness) > BRIGHTNESS_CHANGE_THRESHOLD) {
      curBrightness += (desiredBrightness - curBrightness) * BRIGHTNESS_CHANGE_SPEED;
    }
    
    double maxBrightness = min(1.0, AMPS_PER_STRIP_MAX / powerDraw);
    if(curBrightness > maxBrightness) {
      curBrightness = maxBrightness;
    }
    
    int brightness = (int)(curBrightness * 255);
    brightness = min(brightness, MAX_BRIGHTNESS);
    FastLED.setBrightness(brightness);
  }
}

unsigned long output_start_ms;
unsigned long last_frame_ms;
int frameCount = 0;
void outputLeds() {

/*  int reference_led = NUM_LEDS_LEFT / 2;
  for(int i = 0; i < NUM_LEDS_LEFT; i++) {
    leds_left[i] = leds_left[reference_led];
  }*/
  

  unsigned long elapsed_ms = millis() - output_start_ms;
  if(elapsed_ms < 15000) {
    double blackout_ratio = (15000.0 - elapsed_ms) / 15000.0;
    int blackout_index = NUM_LEDS_LEFT - (int)(blackout_ratio * NUM_LEDS_LEFT);
    for(; blackout_index < NUM_LEDS_LEFT; blackout_index++) {
      leds_left[blackout_index] = CRGB::Black;
    }
  }

  setAutoBrightness();
  
  FastLED.show();
  frameCount++;
  if(millis() - last_frame_ms > 1000) {
    last_frame_ms = millis();
    Serial.print("Frame rate: ");
    Serial.println(frameCount);
    frameCount = 0;
    Serial.print("Brightness: ");
    Serial.println(curBrightness);
  }
}

void playTakeoverAndStay(unsigned long duration_ms) {
  unsigned long end_ms = millis() + duration_ms;
  unsigned long fade_interval_min_ms = TAKEOVER_STAY_INTERVAL_MIN + random(0, TAKEOVER_STAY_INTERVAL_RANGE);
  unsigned long fade_interval_max_ms = TAKEOVER_STAY_INTERVAL_MIN + random(0, TAKEOVER_STAY_INTERVAL_RANGE);

  while(fade_interval_min_ms >= fade_interval_max_ms) {
    fade_interval_min_ms = TAKEOVER_STAY_INTERVAL_MIN + random(0, TAKEOVER_STAY_INTERVAL_RANGE);
    fade_interval_max_ms = TAKEOVER_STAY_INTERVAL_MIN + random(0, TAKEOVER_STAY_INTERVAL_RANGE);
  }

  int prevHue = -1000;

  unsigned long cycle_start_ms, cycle_end_ms;
  CHSV color;
  CRGB prevColor = CRGB::Black, blendColor;
  
  double radius, center, minValue;
  while(millis() < end_ms) {
    cycle_start_ms = millis();
    cycle_end_ms = cycle_start_ms + random(fade_interval_min_ms, fade_interval_max_ms);

    while(hueDist(color.hue, prevHue) < TAKEOVER_STAY_MIN_HUE_DIST) {
      color.hue = random(0, 256);
    }
    prevHue = color.hue;
    color.saturation = random(SATURATION_MIN, 256);
    color.value = 255;
    center = random(0,100000)/100000.0 * (float)NUM_LEDS_LEFT;

    minValue = 0.0;
    while(millis() < cycle_end_ms && minValue < TAKEOVER_STAY_MIN_VALUE_THRESHOLD) {
      radius = (sin(PI/2.0*(float)(millis() - cycle_start_ms) / (float)(cycle_end_ms - cycle_start_ms)) * 2.0) * (float)NUM_LEDS_LEFT;

      minValue = 1.0;
      for(int i = 0; i < NUM_LEDS_LEFT; i++) {
        double dist = min(max(0.0, abs((float)i - center)/radius*2.0-1.0), 1.0);
        double value = 1.0 - dist;

        minValue = min(minValue, value);

        blendColor.red = (int)(CRGB(color).red * value) + (int)(prevColor.red * (1.0-value));
        blendColor.green = (int)(CRGB(color).green * value) + (int)(prevColor.green * (1.0-value));
        blendColor.blue = (int)(CRGB(color).blue * value) + (int)(prevColor.blue * (1.0-value));

        leds_left[i] = blendColor;
        leds_right[i] = blendColor;

      }

      outputLeds();
      FastLED.delay(10);
    }

    prevColor = color;
  }

  cycle_start_ms = millis();
  cycle_end_ms = cycle_start_ms + random(fade_interval_min_ms, fade_interval_max_ms);

  color.hue = random(0, 256);
  color.saturation = random(SATURATION_MIN, 256);
  color.value = 0;
  center = random(0,100000)/100000.0 * (float)NUM_LEDS_LEFT;

  while(millis() < cycle_end_ms) {
    radius = (sin(PI/2.0*(float)(millis() - cycle_start_ms) / (float)(cycle_end_ms - cycle_start_ms)) * 2.0) * (float)NUM_LEDS_LEFT;

    for(int i = 0; i < NUM_LEDS_LEFT; i++) {
      double dist = min(max(0.0, abs((float)i - center)/radius*2.0-1.0), 1.0);
      double value = 1.0 - dist;
 
      blendColor.red = (int)(CRGB(color).red * value) + (int)(prevColor.red * (1.0-value));
      blendColor.green = (int)(CRGB(color).green * value) + (int)(prevColor.green * (1.0-value));
      blendColor.blue = (int)(CRGB(color).blue * value) + (int)(prevColor.blue * (1.0-value));

      leds_left[i] = blendColor;
      leds_right[i] = blendColor;

    }

    outputLeds();
  }
}

void playTakeoverAndFade(unsigned long duration_ms) {
  unsigned long end_ms = millis() + duration_ms;
  unsigned long fade_interval_min_ms = TAKEOVER_FADE_INTERVAL_MIN + random(0, TAKEOVER_FADE_INTERVAL_RANGE);
  unsigned long fade_interval_max_ms = TAKEOVER_FADE_INTERVAL_MIN + random(0, TAKEOVER_FADE_INTERVAL_RANGE);

  while(fade_interval_min_ms >= fade_interval_max_ms) {
    fade_interval_min_ms = TAKEOVER_FADE_INTERVAL_MIN + random(0, TAKEOVER_FADE_INTERVAL_RANGE);
    fade_interval_max_ms = TAKEOVER_FADE_INTERVAL_MIN + random(0, TAKEOVER_FADE_INTERVAL_RANGE);
  }

  int minHue = random(0, 256);
  int maxHue = random(minHue, minHue + 256);

  unsigned long cycle_start_ms, cycle_end_ms;
  CHSV color;
  double radius, center;
  while(millis() < end_ms) {
    cycle_start_ms = millis();
    cycle_end_ms = cycle_start_ms + random(fade_interval_min_ms, fade_interval_max_ms);

    color.hue = random(minHue, maxHue) % 256;
    color.saturation = random(SATURATION_MIN, 256);
    center = random(0,100000)/100000.0 * (float)NUM_LEDS_LEFT;

    while(millis() < cycle_end_ms) {
      radius = sin(PI*(float)(millis() - cycle_start_ms) / (float)(cycle_end_ms - cycle_start_ms)) * (float)NUM_LEDS_LEFT;

      for(int i = 0; i < NUM_LEDS_LEFT; i++) {
        color.value = floor(cos(min(abs((float)i - center)/radius, 1.0)*PI*0.5)*255.0);
        leds_left[i] = color;
        leds_right[i] = color;

      }

      outputLeds();
    }    
  }
}

void playCyclingColor(unsigned long duration_ms) {
  unsigned long end_ms = millis() + duration_ms;

  CHSV color;
  color.saturation = 200;
  color.value = 255;
  unsigned long change_ms = millis();
  while(millis() < end_ms) {
    if(millis() - change_ms > 50) {
      color.hue = (color.hue + 1) % 256;
      change_ms = millis();
    }

    for(int i = 0; i < NUM_LEDS_LEFT; i++) {
        leds_left[i] = color;

    }

    outputLeds();   
  }
}

// LOOP & SETUP

void setup() {
  FastLED.addLeds<SK9822, DATA_PIN, CLOCK_PIN, RGB, DATA_RATE_MHZ(2)>(leds_left, NUM_LEDS_LEFT);

  output_start_ms = millis();
  last_frame_ms = millis();

  randomSeed(analogRead(0));
  Serial.begin(9600);
}

int lastAnimation = -1;
void loop() {
  unsigned long next_duration_ms = DURATION_MIN + random(0, DURATION_RANGE);

  int nextAnimation = lastAnimation;
  while(nextAnimation == lastAnimation) {
    nextAnimation = random(0, 5);
  }
  lastAnimation = nextAnimation;

  switch(nextAnimation) {
    case 0:
      //playRedGreenWorms(next_duration_ms);
      break;
    case 1:
      //playBlueWorms(next_duration_ms);
      break;
    case 2:
      playTakeoverAndFade(next_duration_ms);
      break;
    case 3:
      playTakeoverAndStay(next_duration_ms);
      break;
    case 4:
      playCyclingColor(next_duration_ms);
      //playRandomWorms(next_duration_ms);
      break;
    default:
      break;
  }
}
