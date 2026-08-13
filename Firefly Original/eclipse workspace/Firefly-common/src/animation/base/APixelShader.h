/*
 * APostfilter.h
 *
 *  Created on: Aug 12, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_BASE_APIXELSHADER_H_
#define SRC_ANIMATION_BASE_APIXELSHADER_H_

#include <animation/base/ArbitraryMap.h>
#include <map>
#include <glm/glm.hpp>

class APixelShader {
protected:
	int id;

public:
	APixelShader(int id = 0)
		: id(id)
	{};
	virtual ~APixelShader();

	virtual glm::vec4 renderPixel(glm::vec3 pos, glm::vec4 color, ArbitraryMap* details) =0;
};

#endif /* SRC_ANIMATION_BASE_APIXELSHADER_H_ */
