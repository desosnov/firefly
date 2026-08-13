/*
 * FireflyUtils.h
 *
 *  Created on: Feb 7, 2016
 *      Author: Denis
 */


#ifndef SRC_COMMON_FIREFLYUTILS_H_
#define SRC_COMMON_FIREFLYUTILS_H_

#include <string>
#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <iostream>
#include <glm/glm.hpp>

// GLFW
#define GLFW_INCLUDE_GLU
#include <GLFW/glfw3.h>

// My stuff
#include "Camera.h"
#include "stage/Pixel.h"
#include "stage/PixelStage.h"

#define M_PI 3.14159265358979323846

using namespace std;

#if defined _WIN32 || defined _WIN64
static ostringstream converter;
static string to_string(int input) {
	converter << input;
	string output = converter.str();
	converter.str("");
	return output;
}

static string to_string(double input) {
	converter << input;
	string output = converter.str();
	converter.str("");
	return output;
}
#endif

static void log(string msg)
{
	cout << msg << endl;
}

static string vec3_string(glm::vec3 vect) {
	return to_string(vect.x) + " " + to_string(vect.y) + " " + to_string(vect.z);
}

static void drawAxes()
{
	glLineWidth(2.0f);

	// Red X-axis
	glColor3f(1.0f, 0.0f, 0.0f);
	glBegin(GL_LINES);
	glVertex3f(0.0, 0.0, 0.0);
	glVertex3f(1.0, 0.0, 0.0);
	glEnd();

	// Green Y-axis
	glColor3f(0.0, 1.0, 0.0);
	glBegin(GL_LINES);
	glVertex3f(0.0, 0.0, 0.0);
	glVertex3f(0.0, 1.0, 0.0);
	glEnd();

	// Blue Z-axis
	glColor3f(0.0, 0.0, 1.0);
	glBegin(GL_LINES);
	glVertex3f(0.0, 0.0, 0.0);
	glVertex3f(0.0, 0.0, 1.0);
	glEnd();
}

static void drawMovingPoint(double time)
{
	glPointSize(20.0f);

	glBegin(GL_POINTS);
		glColor3f(1.0f, (cos(time) + 1.0f)/2.0f, 0.0f);
		glVertex3f(0.8f, 0.8f+cos(time), 0.0f);
	glEnd();
}

static void drawSpinningTriangle(double time)
{
	glRotatef(time * 50.f, 0.f, 0.f, 1.f);

	glBegin(GL_TRIANGLES);
		glColor3f(1.f, 0.f, 0.f);
		glVertex3f(-0.6f, -0.4f, sin(time*1.37)/3.0);
		glColor3f(0.f, 1.f, 0.f);
		glVertex3f(0.6f, -0.4f, sin(time*1.37)/3.0);
		glColor3f(0.f, 0.f, 1.f);
		glVertex3f(0.f, 0.6f, sin(time*1.37)/3.0);
	glEnd();

	glRotatef(-time * 50.f, 0.f, 0.f, 1.f);
}

static double rand1() {
	return (double)rand()/(double)RAND_MAX;
}

static double rand(double min, double max) {
	return rand1()*(max-min) + min;
}

#endif /* SRC_COMMON_FIREFLYUTILS_H_ */
