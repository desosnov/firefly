/*
 * Pixel.cpp
 *
 *  Created on: Jan 11, 2016
 *      Author: Denis
 */

#include "stage/Pixel.h"
#include <glm/glm.hpp>

#if defined _WIN32 || defined _WIN64
#include <GL/gl.h>
#else
#include <OpenGL/gl.h>

#endif

#include <math.h>

#define DEFAULT_COLOR (glm::vec3(0.5, 0.7, 0.7))

#define PIXEL_SLICES 8
#define PIXEL_STACKS 3

Pixel::~Pixel() {
	// TODO Auto-generated destructor stub
}

Pixel::Pixel() {
	pos = glm::vec3(0.0, 0.0, 0.0);
	color = DEFAULT_COLOR;
}

Pixel::Pixel(glm::vec3 pos) {
	this->pos = pos;
	this->color = DEFAULT_COLOR;
}

Pixel::Pixel(glm::vec3 pos, glm::vec3 color) {
	this->pos = pos;
	this->color = color;
}

void Pixel::render(double radius)
{
	glColor3f(color.r, color.g, color.b);
	drawSphere(pos, radius, PIXEL_SLICES, PIXEL_STACKS);
}

void Pixel::setColor(glm::vec3 color)
{
	this->color = color;
}

void Pixel::drawSphere(glm::vec3 pos, double radius, int slices, int stacks)
{
	double horiz, vert;
	double horizInterval = M_PI*2.0/(double)slices;
	double vertInterval = M_PI/(double)stacks;

	horiz = 0.0;
	vert = -M_PI/2.0;

	// Triangle fan around bottom tip
	glBegin(GL_TRIANGLE_FAN);
	glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
	vert += vertInterval;
	for(int slice = 0; slice < slices; slice++)
	{
		glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
		horiz += horizInterval;
	}
	glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
	glEnd();

	// Middle (slices-2) slices
	for(int stack = 1; stack < stacks - 1; stack++)
	{
		glBegin(GL_TRIANGLE_STRIP);
		for(int slice = 0; slice < slices; slice++)
		{
			glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
			vert += vertInterval;
			glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
			vert -= vertInterval;
			horiz += horizInterval;
		}
		glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
		vert += vertInterval;
		glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
		glEnd();
	}

	// Triangle fan around top tip
	glBegin(GL_TRIANGLE_FAN);
	vert += vertInterval;
	glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
	vert -= vertInterval;
	for(int slice = 0; slice < slices; slice++)
	{
		glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
		horiz += horizInterval;
	}
	glVertex3f(pos.x+cos(horiz)*cos(vert)*radius, pos.y+sin(horiz)*cos(vert)*radius, pos.z+sin(vert)*radius);
	glEnd();

}
