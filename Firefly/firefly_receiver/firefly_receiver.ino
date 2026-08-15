// Firefly receiver — one sketch, two boards.
//
// Compiles for the original Teensy 3.2 (USB serial only) and for the ESP32-S3
// (USB serial *plus* wifi). The LED output, brightness fading and screensaver are
// shared; only the input differs, which is the same split the desktop app makes
// between Serial and WifiTransport.
//
// Select the board in the Arduino IDE and it configures itself.
//
// Wifi flow on the S3:
//   No credentials stored  -> brings up its own access point "Firefly-XXXX".
//                             GET /firefly            identify
//                             GET /provision?ssid=&pass=  store and join
//   Credentials stored     -> joins that network, answers "FIREFLY?" broadcast
//                             probes on UDP 21324 and receives pixel frames there.
//
// Pixel packet layout, mirrored from Transport.cs:
//   0      'F'    magic
//   1      frame sequence
//   2      chunk index
//   3      chunk count
//   4..5   first pixel index, big endian
//   6..7   pixel count in this chunk, big endian
//   8..    RGB triples

// Note: FASTLED_FORCE_SOFTWARE_SPI is a pre-Channels-API macro and this
// FastLED version ignores it — the driver is pinned explicitly below instead,
// via the fl::Bus::BIT_BANG template argument on addLeds<>().

#include "FastLED.h"

// ------------------------------ BOARD CONFIG ------------------------------

#if defined(ARDUINO_TEENSY32) || defined(__MK20DX256__)
  #define FIREFLY_BOARD_TEENSY 1
  #define DATA_PIN   11
  #define CLOCK_PIN  13
#elif defined(CONFIG_IDF_TARGET_ESP32S3) || defined(ARDUINO_ESP32S3_DEV)
  #define FIREFLY_BOARD_ESP32 1
  // Adjust to match how the strip is wired to the devkit.
  #define DATA_PIN   11
  #define CLOCK_PIN  47
#else
  #error "Unrecognised board. Select Teensy 3.2 or an ESP32-S3 target."
#endif

#define NUM_LEDS 1440
#define WORM_FIELD_SIZE (NUM_LEDS)
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

// How long without pixels before falling back to the screensaver.
#define INPUT_TIMEOUT_MS 2000

CRGB leds[NUM_LEDS];
CHSV strobes[NUM_STROBES];
int strobeLocations[NUM_STROBES];
bool strobeMap[NUM_LEDS];

// ------------------------------- UTILITIES --------------------------------

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

// ---------------------------- STATUS INDICATOR -----------------------------
//
// Pixels 0 and 1 double as a connectivity indicator: dark->bright->dark over
// 30 frames, in a colour set by ledStatus. RECEIVING leaves them alone so
// real pixel data isn't clobbered once frames are actually flowing.

enum LedStatus { SOFT_AP, WIFI_SET, WIFI_CONNECTED, RECEIVING };
LedStatus ledStatus = WIFI_CONNECTED;
unsigned long statusFrame = 0;

void applyStatusIndicator() {
  if (ledStatus == RECEIVING) return;

  CRGB color = CRGB::Black;
  if (ledStatus == SOFT_AP)             color = CRGB::Blue;
  else if (ledStatus == WIFI_SET)       color = CRGB::Yellow;
  else if (ledStatus == WIFI_CONNECTED) color = CRGB::White;

  int phase = statusFrame % 60;
  statusFrame++;
  int level = phase < 30 ? (phase * 100) / 29 : 100 - (((phase - 30) * 100) / 29);

  CRGB pulsed = color;
  pulsed.red   = pulsed.red   * level / 255;
  pulsed.green = pulsed.green * level / 255;
  pulsed.blue  = pulsed.blue  * level / 255;

  // This strip's wiring needs red/blue swapped to show true colour — the same
  // compensation setPixel() applies to incoming frame data (0 Facts.md §3.1).
  leds[0] = CRGB(pulsed.blue, pulsed.green, pulsed.red);
  leds[1] = CRGB(pulsed.blue, pulsed.green, pulsed.red);
}

// ------------------------------- SHOW LEDS --------------------------------

unsigned long last_frame_ms;

void showLeds() {
  if (fading)
    calcBrightness();

  for (int i = 0; i < NUM_LEDS; i++) {
    leds[i].red = leds[i].red * bright;
    leds[i].green = leds[i].green * bright;
    leds[i].blue = leds[i].blue * bright;
  }

  applyStatusIndicator();

  FastLED.show();
}

// --------------------------------- WORM -----------------------------------

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
    leds[li].red = min(255, leds[li].red + pixel_rgb.red);
    leds[li].blue = min(255, leds[li].blue + pixel_rgb.blue);
    leds[li].green = min(255, leds[li].green + pixel_rgb.green);
  }

  wormPos += WORM_SPEED;
}

// -------------------------------- STROBE ----------------------------------

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

void renderScreensaver() {
  clearLeds();
  renderStrobe();
  showLeds();
}

// ------------------------------ PIXEL INPUT -------------------------------
//
// Channel order is deliberately odd and deliberately preserved. The original
// read three bytes as b, g, r and assigned red from the *third*, so the desktop's
// red channel lands in blue. Both transports apply the same mapping, so wifi and
// USB produce identical output. Fixing it means changing both ends together.

inline void setPixel(int i, uint8_t byte0, uint8_t byte1, uint8_t byte2) {
  leds[i].red   = byte2;
  leds[i].green = byte1;
  leds[i].blue  = byte0;
}

// Returns true if a complete frame was read from USB.
bool readSerialFrame() {
  if (Serial.available() < 3) return false;

  unsigned long deadline = millis() + INPUT_TIMEOUT_MS;
  for (int i = 0; i < NUM_LEDS; i++) {
    while (Serial.available() < 3) {
      if (millis() > deadline) return false;   // partial frame; drop it
    }
    uint8_t b0 = (uint8_t)Serial.read();
    uint8_t b1 = (uint8_t)Serial.read();
    uint8_t b2 = (uint8_t)Serial.read();
    setPixel(i, b0, b1, b2);
    deadline = millis() + INPUT_TIMEOUT_MS;
  }
  return true;
}

// ------------------------------ WIFI (ESP32) ------------------------------

#if FIREFLY_BOARD_ESP32

#include <WiFi.h>
#include <WiFiUdp.h>
#include <WebServer.h>
#include <Preferences.h>

#define FIREFLY_PORT 21324
#define PACKET_MAGIC 'F'
#define PACKET_HEADER 8
#define MAX_PACKET (PACKET_HEADER + 400 * 3)

Preferences prefs;
WiFiUDP udp;
WebServer server(80);

String deviceName;
bool provisioned = false;
uint8_t packet[MAX_PACKET];

// Assembled here and only copied to leds[] once every chunk of a frame has
// arrived, so a dropped packet can't tear a frame across two updates.
uint8_t frameBuf[NUM_LEDS * 3];
uint8_t assemblingSeq = 0;
uint8_t chunksSeen = 0;
uint8_t chunksExpected = 0;

void makeDeviceName() {
  uint64_t mac = ESP.getEfuseMac();
  char buf[20];
  snprintf(buf, sizeof(buf), "Firefly-%04X", (uint16_t)(mac & 0xFFFF));
  deviceName = String(buf);
}

void handleIdentify() {
  server.send(200, "text/plain", deviceName + "|" + String(NUM_LEDS));
}

void handleProvision() {
  String ssid = server.arg("ssid");
  String pass = server.arg("pass");

  if (ssid.length() == 0) {
    server.send(400, "text/plain", "ERR no ssid");
    return;
  }

  prefs.begin("firefly", false);
  prefs.putString("ssid", ssid);
  prefs.putString("pass", pass);
  prefs.end();

  server.send(200, "text/plain", "OK");
  delay(500);          // let the reply flush before the radio switches mode
  ESP.restart();
}

void startSoftAP() {
  WiFi.mode(WIFI_AP);
  WiFi.softAP(deviceName.c_str());

  Serial.println("Starting soft AP " + deviceName);

  server.on("/firefly", handleIdentify);
  server.on("/provision", handleProvision);
  server.begin();

  provisioned = false;
  ledStatus = SOFT_AP;
}

bool startStation() {
  prefs.begin("firefly", true);
  String ssid = prefs.getString("ssid", "");
  String pass = prefs.getString("pass", "");
  prefs.end();

  if (ssid.length() == 0) return false;

  ledStatus = WIFI_SET;

  WiFi.mode(WIFI_STA);
  WiFi.setSleep(false);            // latency matters more than power here
  WiFi.begin(ssid.c_str(), pass.c_str());

  unsigned long deadline = millis() + 15000;
  while (WiFi.status() != WL_CONNECTED && millis() < deadline) {
    renderScreensaver();
  }

  if (WiFi.status() != WL_CONNECTED) return false;

  udp.begin(FIREFLY_PORT);
  provisioned = true;
  ledStatus = WIFI_CONNECTED;
  return true;
}

// Answers discovery probes and assembles pixel frames.
// Returns true if a complete frame arrived.
bool readWifiFrame() {
  bool complete = false;
  int size;

  while ((size = udp.parsePacket()) > 0) {
    int len = udp.read(packet, MAX_PACKET);
    if (len <= 0) continue;

    // Discovery probe
    if (len >= 8 && memcmp(packet, "FIREFLY?", 8) == 0) {
      String reply = "FIREFLY!" + deviceName + "|" + String(NUM_LEDS);
      udp.beginPacket(udp.remoteIP(), udp.remotePort());
      udp.write((const uint8_t*)reply.c_str(), reply.length());
      udp.endPacket();
      continue;
    }

    if (len < PACKET_HEADER || packet[0] != PACKET_MAGIC) continue;

    uint8_t seq        = packet[1];
    uint8_t chunkIndex = packet[2];
    uint8_t chunkCount = packet[3];
    int first = (packet[4] << 8) | packet[5];
    int count = (packet[6] << 8) | packet[7];

    if (first < 0 || count < 0 || first + count > NUM_LEDS) continue;
    if (len < PACKET_HEADER + count * 3) continue;

    if (seq != assemblingSeq) {      // new frame; abandon any partial one
      assemblingSeq = seq;
      chunksSeen = 0;
      chunksExpected = chunkCount;
    }

    memcpy(frameBuf + first * 3, packet + PACKET_HEADER, count * 3);
    chunksSeen++;

    if (chunksSeen >= chunksExpected) {
      for (int i = 0; i < NUM_LEDS; i++) {
        setPixel(i, frameBuf[i*3], frameBuf[i*3+1], frameBuf[i*3+2]);
      }
      chunksSeen = 0;
      complete = true;
    }
  }

  return complete;
}

#endif  // FIREFLY_BOARD_ESP32

// --------------------------------- MAIN -----------------------------------

void fadeToInput() {
  fadeBrightness(0.0, 1500L);

  while (bright > 0.0) {
    renderScreensaver();
  }

  fadeBrightness(1.0, 3000L);
}

bool readAnyFrame() {
#if FIREFLY_BOARD_ESP32
  if (provisioned && readWifiFrame()) { ledStatus = RECEIVING; return true; }
  if (!provisioned) server.handleClient();
#endif
  if (readSerialFrame()) { ledStatus = RECEIVING; return true; }
  return false;
}

void setup() {
  randomSeed(analogRead(0));
  last_frame_ms = millis();

  Serial.begin(COM_BAUD);

#if FIREFLY_BOARD_ESP32
  // This board doesn't auto-reset when the Serial Monitor attaches, so without
  // this wait, early boot prints fire before the monitor connects and are lost.
  // Caps at 3s so it still boots fine with no computer attached.
  unsigned long serialWaitStart = millis();
  while (!Serial && millis() - serialWaitStart < 3000) {
    delay(10);
  }
#endif

  Serial.println("Start setup");
  #if FIREFLY_BOARD_ESP32
    Serial.println("ESP32 S3 board");
  #endif
  Serial.println(CLOCK_PIN);

  // fl::Bus::BIT_BANG pins the portable cycle-counted GPIO driver explicitly.
  // Without it, FastLED's auto-selection picks the highest-priority driver
  // that claims to handle SPI chipsets — on the S3 that's LCD_SPI (priority
  // 10), which has a known esp_cache_msync/DMA bug that hangs mid-frame and
  // trips the interrupt watchdog. BIT_BANG (priority 0) skips DMA entirely.
  FastLED.addLeds<SK9822, DATA_PIN, CLOCK_PIN, RGB, DATA_RATE_MHZ(7)>(leds, NUM_LEDS);

  clearLeds();
  showLeds();
  delay(1000);

  // Started before the WiFi connect attempt below, which can block
  // renderScreensaver() calls for up to 15s — otherwise brightness sits at
  // its initial 0.0 for that whole window and the screensaver is invisible.
  fadeBrightness(1.0, 3500L);

#if FIREFLY_BOARD_ESP32
  makeDeviceName();
  if (!startStation()) {
    startSoftAP();
  }
#endif

  while (!readAnyFrame()) {
    renderScreensaver();
  }

  fadeToInput();
}

// Holds the last complete frame rather than blanking when input stutters, and
// only falls back to the screensaver once nothing has arrived for
// INPUT_TIMEOUT_MS. The original had no hold — a gap dropped it straight out.
unsigned long lastFrame_ms = 0;
bool showingInput = false;

void loop() {
  if (readAnyFrame()) {
    lastFrame_ms = millis();
    if (!showingInput) {
      fadeToInput();
      showingInput = true;
    }
    showLeds();
    return;
  }

    if (showingInput && millis() - lastFrame_ms < INPUT_TIMEOUT_MS) {
      showLeds();          // hold the last frame
      return;
    }

    if (showingInput) {
      fadeBrightness(0.0, 3500L);
      unsigned long stopTime = millis() + 3500L;
      while (!readAnyFrame() && millis() < stopTime) {
        showLeds();
    }
    clearLeds();
    fadeBrightness(1.0, 5000L);
    showingInput = false;
    #if FIREFLY_BOARD_ESP32
      if (provisioned) ledStatus = WIFI_CONNECTED;
    #endif
  }

  renderScreensaver();
}
