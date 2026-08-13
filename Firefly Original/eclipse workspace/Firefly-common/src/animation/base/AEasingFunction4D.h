/*
 * AEasingFunction4D.h
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_BASE_AEASINGFUNCTION4D_H_
#define SRC_ANIMATION_BASE_AEASINGFUNCTION4D_H_

#include <glm/glm.hpp>
#include <vector>
#include <algorithm>

class AEasingFunction4D {
protected:
	std::vector<glm::vec4*> bindings;
	bool finishedFlag = false;

	double normalizeInput(double input);

public:
	double start, finish;
	glm::vec4 easeFrom, easeTo;

	AEasingFunction4D(
			double start,
			double finish,
			glm::vec4 easeFrom,
			glm::vec4 easeTo)
			: start(start),
			  finish(finish),
			  easeFrom(easeFrom),
			  easeTo(easeTo) {};
	virtual ~AEasingFunction4D();

	void bindValue(glm::vec4* valPtr);
	void update(double time);
	bool finished();

	virtual glm::vec4 easedValue(double input) =0;
};
#endif /* SRC_ANIMATION_BASE_AEASINGFUNCTION4D_H_ */
