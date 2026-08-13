/*
 * Primitive.cpp
 *
 *  Created on: Aug 4, 2016
 *      Author: Denis
 */

#include <animation/base/APrimitive.h>

APrimitive::APrimitive() {
	// TODO Auto-generated constructor stub

}

APrimitive::~APrimitive() {
	// TODO Auto-generated destructor stub
}

glm::vec4 APrimitive::renderPixel(glm::vec3 pos, ArbitraryMap* details) {
	return renderPixelAt(pos, details);
}
