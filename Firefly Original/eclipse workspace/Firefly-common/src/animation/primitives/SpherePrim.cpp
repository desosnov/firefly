/*
 * SpherePrim.cpp
 *
 *  Created on: Aug 9, 2016
 *      Author: Denis
 */

#include <animation/primitives/SpherePrim.h>
#include "FireflyUtils.h"

glm::vec4 SpherePrim::renderAtDistanceFromPoint(double distance, ArbitraryMap* details) {
	float range = distance/radius;
	if(range <= 1.0) {
		details->setInt("shaderIndex", shaderIndex);
		return colorPattern->getColor(1.0f-(distance/radius)*(distance/radius));
	} else if(range < 1.0 + SP_SOFT_EDGE_RATIO) {
		float alpha = 1.0 - (distance/radius - 1.0)/SP_SOFT_EDGE_RATIO;
		glm::vec4 color = colorPattern->getColor(0.0f);
		color *= alpha;
		color.a = 1.0;
		return color;
	} else if(range < 1.0 + SP_SOFT_EDGE_RATIO + 0.1) {
		float alpha = sin(1.0 - (range - 1.0 - SP_SOFT_EDGE_RATIO)/0.1)*M_PI/2.0;
		glm::vec4 color = glm::vec4(0.0, 0.0, 0.0, alpha);
		return color;
	} else {
		return glm::vec4(0.0, 0.0, 0.0, 0.0);
	}
}

SpherePrim::~SpherePrim() {
	// TODO Auto-generated destructor stub
}

