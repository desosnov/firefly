/*
 * EasingFunction3D.h
 *
 *  Created on: Mar 11, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_EASING_EASINGFUNCTION3D_H_
#define SRC_ANIMATION_EASING_EASINGFUNCTION3D_H_

#include <glm/glm.hpp>
#include <vector>
#include <algorithm>

class AEasingFunction3D {
protected:
	std::vector<glm::vec3*> bindings;
	bool finishedFlag = false;

	double normalizeInput(double input);

public:
	double start, finish;
	glm::vec3 easeFrom, easeTo;

	AEasingFunction3D(
			double start,
			double finish,
			glm::vec3 easeFrom,
			glm::vec3 easeTo)
			: start(start),
			  finish(finish),
			  easeFrom(easeFrom),
			  easeTo(easeTo) {};
	virtual ~AEasingFunction3D();

	void bindValue(glm::vec3* valPtr);
	void update(double time);
	bool finished();

	virtual glm::vec3 easedValue(double input) =0;
};

#endif /* SRC_ANIMATION_EASING_EASINGFUNCTION3D_H_ */
