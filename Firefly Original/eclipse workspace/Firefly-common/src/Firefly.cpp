//============================================================================
// Name        : Firefly.cpp
// Author      : Denis Sosnovtsev
// Version     :
// Copyright   : Nothing is forbidden
// Description : Hello World in C++, Ansi-style
//============================================================================

// Standards

#include <stdlib.h>
#include <ctime>

// To shut up the deprecation warnings
#define GL_SILENCE_DEPRECATION

// GLFW
#define GLFW_INCLUDE_GLU
#include <GLFW/glfw3.h>

// My stuff
#include "Camera.h"
#include "stage/Pixel.h"
#include "stage/PixelStage.h"
#include "FireflyController.h"

using namespace std;

#define WINDOW_WIDTH 1920 //1920
#define WINDOW_HEIGHT 1280 // 1280
#define WINDOW_TITLE "Firefly Controller"
#define COM_PORT "COM9"

int main()
{
	int randVal = (int)(time(NULL))%123;
	for(int i = 0; i < randVal; i++)
		rand();

	FireflyController ffc =
			FireflyController(
					WINDOW_TITLE,
					WINDOW_WIDTH,
					WINDOW_HEIGHT,
					COM_PORT,
					FIREFLY_V2_CYLINDER);

	ffc.start();
	exit(EXIT_SUCCESS);
}
