/*
 * Pixel.h
 *
 *  Created on: Jan 11, 2016
 *      Author: Denis
 */

#ifndef PIXEL_H_
#define PIXEL_H_

#include <glm/glm.hpp>

class Pixel {
private:
	glm::vec3 pos, color;
public:
	Pixel();
	Pixel(glm::vec3 pos);
	Pixel(glm::vec3 pos, glm::vec3 color);
	virtual ~Pixel();

	static void drawSphere(glm::vec3 pos, double radius, int slices, int stacks);

	void render(double radius);
	void setColor(glm::vec3 color);

	glm::vec3 getPos() {
		return pos;
	}

	glm::vec3 getColor() {
		return color;
	}

	double getX() const {
		return pos.x;
	}

	double getY() const {
		return pos.y;
	}

	double getZ() const {
		return pos.z;
	}
};

#endif /* PIXEL_H_ */
