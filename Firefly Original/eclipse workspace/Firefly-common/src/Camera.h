/*
 * Camera.h
 *
 *  Created on: Jan 9, 2016
 *      Author: Denis
 */

#ifndef CAMERA_H_
#define CAMERA_H_

#define DEFAULT_HOR 45.0
#define DEFAULT_VER 45.0
#define DEFAULT_X 0.0
#define DEFAULT_Y 0.0
#define DEFAULT_Z 0.0
#define DEFAULT_DIST 3.5
#define MIN_DIST 1.5
#define MAX_DIST 10.0
#define ZOOM_MULT 1.1
#define MAX_VERTICAL 80.0
#define MIN_VERTICAL -80.0

#define PI 3.14159265359

#include <glm/glm.hpp>

class Camera {
private:
	double hor = DEFAULT_HOR, ver = DEFAULT_VER;
	glm::vec3 pos = {DEFAULT_X, DEFAULT_Y, DEFAULT_Z};
	double dist = DEFAULT_DIST;

public:
	Camera();
	virtual ~Camera();

	glm::vec3 getPos() {
		return pos;
	}

	void setPerspective();
	void rotate(double horiz_move, double vert_move);
	void moveTo(glm::vec3 newPos);
	void zoom(double distCloser);
	void zoomIn();
	void zoomOut();
};


#endif /* CAMERA_H_ */
