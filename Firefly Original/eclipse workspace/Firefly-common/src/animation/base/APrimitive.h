/*
 * Primitive.h
 *
 *  Created on: Aug 4, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_BASE_APRIMITIVE_H_
#define ANIMATION_BASE_APRIMITIVE_H_

#include <animation/base/AAnimation.h>
#include <glm/glm.hpp>
#include "stage/Pixel.h"

class APrimitive {
protected:
	virtual glm::vec4 renderPixelAt(glm::vec3 pos, ArbitraryMap* details) =0;

public:
	APrimitive();
	virtual ~APrimitive();

	glm::vec4 renderPixel(glm::vec3 pos, ArbitraryMap* details);
};




#endif /* ANIMATION_BASE_APRIMITIVE_H_ */
