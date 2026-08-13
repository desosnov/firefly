/*
 * PostSparkleFilter.h
 *
 *  Created on: Feb 11, 2016
 *      Author: d
 */

#ifndef SRC_POSTSPARKLEFILTER_H_
#define SRC_POSTSPARKLEFILTER_H_

#include <animation/base/APixelShader.h>
#include <map>
#include "glm/glm.hpp"
#include "stage/Pixel.h"

#define SPARKLE_RISE 20
#define SPARKLE_FALL 30
#define SPARKLE_PROPORTION 0.90
#define SPARKLE_CREATE_CHANCE ((int)((SPARKLE_RISE+SPARKLE_FALL)/SPARKLE_PROPORTION))
#define SPARKLE_BRIGHTNESS 1.0
#define SPARKLE_MAX 1.5
#define SPARKLE_MIN 0.0

class SparkleShader : public APixelShader {
private:
	int sparklesToCreate = 0;
	int sparkleRise, sparkleFall, sparkleCreateChance;
	double sparkleProportion, sparkleMax, sparkleMin;

	void applyIntensity(glm::vec4 *color, double intensity);
	char* getStateKey();

public:
	SparkleShader(
			int sparkleRise = SPARKLE_RISE,
			int sparkleFall = SPARKLE_FALL,
			double sparkleProportion = SPARKLE_PROPORTION,
			double sparkleMax = SPARKLE_MAX,
			double sparkleMin = SPARKLE_MIN)
			: sparkleRise(sparkleRise),
			  sparkleFall(sparkleFall),
			  sparkleProportion(sparkleProportion),
			  sparkleMax(sparkleMax),
			  sparkleMin(sparkleMin)
			{ sparkleCreateChance = ((int)((sparkleRise+sparkleFall)/sparkleProportion)); };
	virtual ~SparkleShader();

	glm::vec4 renderPixel(glm::vec3 pos, glm::vec4 color, ArbitraryMap* details);
};

#endif /* SRC_POSTSPARKLEFILTER_H_ */
