// Use if you want to force the software SPI subsystem to be used for some reason (generally, you don't)
// #define FASTLED_FORCE_SOFTWARE_SPI
// Use if you want to force non-accelerated pin access (hint: you really don't, it breaks lots of things)
// #define FASTLED_FORCE_SOFTWARE_SPI
// #define FASTLED_FORCE_SOFTWARE_PINS
#include "FastLED.h"


// How many leds are in the strip?
#define NUM_LEDS 1440
#define WORM_FIELD_SIZE (NUM_LEDS)

// Data pin that led data will be written out over
#define DATA_PIN 11
#define CLOCK_PIN 13
#define COM_BAUD 9600

#define USE_LINEAR_BRIGHTNESS true

#define WORM_LENGTH 201
#define WORM_SPEED 1
#define WORM_COLOR_SPEED 3 // 1 = 60 sec to rotate
#define WORM_COLOR_SIZE (WORM_FIELD_SIZE*3/2)
#define WORM_MAX_BRIGHTNESS 100
#define STROBE_MAX_BRIGHTNESS 150
#define NUM_STROBE_LEVELS 150
#define NUM_STROBE_PHASES (NUM_STROBE_LEVELS*2)
#define NUM_STROBES 600
#define STROBE_SATURATION_MIN 200
#define STROBE_SATURATION_MAX 230
#define HUE_RANGE 10

// This is an array of leds.  One item for each led in your strip.
CRGB leds[NUM_LEDS];
CHSV strobes[NUM_STROBES];
int strobeLocations[NUM_STROBES];
bool strobeMap[NUM_LEDS];



//------------------------------- UTILITIES -------------------------------------
double bright = 0.0;
bool fading = false;
double startBrightness;
double targetBrightness;
unsigned long start_ms, duration_ms;
void fadeBrightness(double target, long duration) {
  startBrightness = bright;
  targetBrightness = target;
  duration_ms = duration;
  start_ms = millis();
  fading = true;
}

void calcBrightness() {
  if (millis() < start_ms + duration_ms) {
    bright = (targetBrightness - startBrightness) * (millis() - start_ms) / duration_ms + startBrightness;
  } else {
    bright = targetBrightness;
    fading = false;
  }
}

void clearLeds() {
  fill_solid(leds, NUM_LEDS, CRGB::Black);
}


//------------------------------- SHOW LEDS -------------------------------------
int frameCount = 0;
unsigned long last_frame_ms;
unsigned long frame_render_ms = 0, frame_copy_ms = 0;
unsigned long frame_copy_start_ms, frame_render_start_ms;

void showLeds() {
  if (fading)
    calcBrightness();

//  if(bright != 1.0) {
    for (int i = 0; i < NUM_LEDS; i++) {
      leds[i].red = leds[i].red * bright;
      leds[i].green = leds[i].green * bright;
      leds[i].blue = leds[i].blue * bright;
    }
//  }
  
//  frame_render_start_ms = millis();
  FastLED.show();
//  frame_render_ms += millis() - frame_render_start_ms;
  
/*  frameCount++;
  if(millis() - last_frame_ms > 1000) {
    Serial.print("Frame rate: ");
    Serial.println(frameCount);
    Serial.print("Average render time = ");
    Serial.print((double)frame_render_ms / frameCount);
    Serial.print("ms | Copy + render = ");
    Serial.println((double)frame_copy_ms / frameCount);
    frameCount = 0;
    frame_render_ms = 0;
    frame_copy_ms = 0;
    last_frame_ms = millis();
  }*/
}

//------------------------------- WORM -------------------------------------
int hueRand = -1;
CHSV baseColorOfPixel(int led) {
  if (hueRand == -1)
    hueRand = random(0, 60000);
  long hue = ((long)(led + WORM_COLOR_SPEED * WORM_COLOR_SIZE * (millis() + hueRand) / 60000) * 255 / WORM_COLOR_SIZE) % 255;

  return CHSV((int)hue, 255, 255);
}

int wormPixelBrightness(int offset) {
  offset = (WORM_LENGTH / 2) - abs(offset - WORM_LENGTH / 2) + 1;

  return (long)offset * WORM_MAX_BRIGHTNESS / (WORM_LENGTH / 2 + 1);
}

unsigned long wormPos = WORM_SPEED;
void renderWorm() {

  for (int wormOffset = 0; wormOffset < WORM_LENGTH; wormOffset++) {
    CHSV pixel = baseColorOfPixel(wormPos*1.1 + wormOffset);
    int pixel_bright = wormPixelBrightness(wormOffset);
    CRGB pixel_rgb = pixel;
    pixel_rgb.red = pixel_rgb.red * pixel_bright / 255;
    pixel_rgb.green = pixel_rgb.green * pixel_bright / 255;
    pixel_rgb.blue = pixel_rgb.blue * pixel_bright / 255;

    int li = (wormPos + wormOffset) % WORM_FIELD_SIZE;
    leds[li].red = MIN(255, leds[li].red + pixel_rgb.red);
    leds[li].blue = MIN(255, leds[li].blue + pixel_rgb.blue);
    leds[li].green = MIN(255, leds[li].green + pixel_rgb.green);
  }

  wormPos += WORM_SPEED;
}

//------------------------------- STROBE -------------------------------------
int emptyStrobeLocation() {
  int strobeLocation;
  
  do {
    strobeLocation = random(0, NUM_LEDS);
  } while (strobeMap[strobeLocation]);
  
  return strobeLocation;
}

int strobePhase = 0;
void renderStrobe() {

  int strobe = strobePhase;
  while (strobe < NUM_STROBES) {
    leds[strobeLocations[strobe]] = CRGB::Black;
    strobeMap[strobeLocations[strobe]] = false;
    strobeLocations[strobe] = emptyStrobeLocation();
    strobeMap[strobeLocations[strobe]] = true;
    strobes[strobe].hue = (baseColorOfPixel(strobeLocations[strobe]).hue + random(-1*HUE_RANGE, HUE_RANGE)) % 255;
    strobes[strobe].saturation = random(STROBE_SATURATION_MIN, STROBE_SATURATION_MAX);
    strobe += NUM_STROBE_PHASES;
  }

  for (int si = 0; si < NUM_STROBES; si++) {
    long valPhase = (si + NUM_STROBE_PHASES - strobePhase) % NUM_STROBE_PHASES;
    strobes[si].val = 255;
    int strobe_bright = (long)(NUM_STROBE_LEVELS - abs(valPhase - NUM_STROBE_LEVELS)) * (long)STROBE_MAX_BRIGHTNESS / (long)NUM_STROBE_LEVELS;
    if(USE_LINEAR_BRIGHTNESS) {
      CRGB strobe_rgb = strobes[si];
      strobe_rgb.red = strobe_rgb.red * strobe_bright / 255;
      strobe_rgb.green = strobe_rgb.green * strobe_bright / 255;
      strobe_rgb.blue = strobe_rgb.blue * strobe_bright / 255;
      leds[strobeLocations[si]] = strobe_rgb;
    } else {
      strobes[si].val = strobe_bright;
      leds[strobeLocations[si]] = strobes[si];
    }
  }

  strobePhase++;
  strobePhase = strobePhase % NUM_STROBE_PHASES;
}

void renderGradient() {
  for (int i = 0; i < NUM_LEDS; i++) {
    if (i/256 == 0) {
      leds[i].red = i % 256;
    } else if (i/256 == 1) {
      leds[i].green = i % 256;
    } else if (i/256 == 2) {
      leds[i].blue = i % 256;
    }
  }
}

//------------------------------- MAINS -------------------------------------
void renderScreensaver() {
  clearLeds();
  renderStrobe();
//  renderWorm();
  showLeds();
}

void fadeToSerial() {
  fadeBrightness(0.0, 1500L);

  while (bright > 0.0) {
    renderScreensaver();
  }

  fadeBrightness(1.0, 3000L);
}


int frames = 0;
unsigned long timeoutReset = 0;
void fireflyReceiverLoop() {
  for (int i = 0; i < NUM_LEDS; i++) {
    timeoutReset = millis() + 2000;
    while (Serial.available() < 3 && millis() < timeoutReset) {}
    if (Serial.available() < 3) {
      i = 0;
      Serial.end();
      Serial.begin(COM_BAUD);
      while (Serial.available() > 0) {
        Serial.read();
      }
      fadeBrightness(0.0, 3500L);
      unsigned long stopTime = millis() + 3500L;
      while (Serial.available() < 3 && millis() < stopTime) {
        showLeds();
      }

      for (int pi = 0; pi < NUM_LEDS; pi++)
        leds[pi] = CRGB::Black;


      fadeBrightness(1.0, 5000L);
      while (Serial.available() < 3) {
        renderScreensaver();
      }
      fadeToSerial();
    }

    int b = (int)Serial.read();
    int g = (int)Serial.read();
    int r = (int)Serial.read();

    leds[i].red = r;
    leds[i].green = g;
    leds[i].blue = b;
  }
  showLeds();
  frames++;
}


// This function sets up the ledsand tells the controller about them
void setup() {
  randomSeed(analogRead(0));
  last_frame_ms = millis();

  Serial.begin(COM_BAUD);

  FastLED.addLeds<SK9822, DATA_PIN, CLOCK_PIN, RGB, DATA_RATE_MHZ(7)>(leds, NUM_LEDS);

  for (int pi = 0; pi < NUM_LEDS; pi++) {
    leds[pi] = CRGB::Black;
  }

  showLeds();
  delay(1000);

  fadeBrightness(1.0, 3500L);

  while (Serial.available() < 3) {
    renderScreensaver();
  }

  fadeToSerial();
}


// This function runs over and over, and is where you do the magic to light
// your leds.
void loop() {
  fireflyReceiverLoop();
}



