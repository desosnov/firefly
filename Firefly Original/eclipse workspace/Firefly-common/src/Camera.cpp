/*
 * Camera.cpp
 *
 *  Created on: Jan 9, 2016
 *      Author: Denis
 */

#include "Camera.h"

#if defined _WIN32 || defined _WIN64
	#include <GL/gl.h>
	#include <GL/glu.h>
#else
	#include <OpenGL/gl.h>
	#include <OpenGL/glu.h>
#endif

#define _USE_MATH_DEFINES
#include <math.h>

Camera::Camera() {
	// TODO Auto-generated constructor stub

}

Camera::~Camera() {
	// TODO Auto-generated destructor stub
}

void Camera::setPerspective() {
	gluLookAt(pos.x + dist*cos(hor*PI/180.0)*cos(ver*PI/180.0),
			  pos.y + dist*sin(hor*PI/180.0)*cos(ver*PI/180.0),
			  pos.z + dist*sin(ver*PI/180.0), // Camera pos, based on z = 1 as up vector
			pos.x, pos.y, pos.z, // Look at pos
			0.0, 0.0, 1.0); // Up vector
}

void Camera::rotate(double horiz_move, double vert_move) {
	hor += horiz_move;
	ver += vert_move;

	if (ver > MAX_VERTICAL)
		ver = MAX_VERTICAL;
	if(ver < MIN_VERTICAL)
		ver = MIN_VERTICAL;
}

void Camera::moveTo(glm::vec3 newPos) {
	pos = newPos;
}

void Camera::zoom(double distCloser) {
	dist -= distCloser;
	if (dist < MIN_DIST)
		dist = MIN_DIST;
	if (dist > MAX_DIST)
		dist = MAX_DIST;
}

void Camera::zoomIn() {
	dist /= ZOOM_MULT;
	if (dist < MIN_DIST)
		dist = MIN_DIST;
	if (dist > MAX_DIST)
		dist = MAX_DIST;

}

void Camera::zoomOut() {
	dist *= ZOOM_MULT;
	if (dist < MIN_DIST)
		dist = MIN_DIST;
	if (dist > MAX_DIST)
		dist = MAX_DIST;
}
