/*
 * PixelShaderSet.h
 *
 *  Created on: Aug 12, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_SETS_PIXELSHADERSET_H_
#define SRC_ANIMATION_SETS_PIXELSHADERSET_H_

#include <map>
#include <animation/base/APixelShader.h>
#include <animation/pixelshaders/SparkleShader.h>

class PixelShaderSet {
protected:
	std::map<double, int> weightedSet;
	double totalWeight = 0.0;
	void add(int creationIndex, double weight);

	virtual APixelShader* createIndex(int index) =0;

public:
	PixelShaderSet();
	virtual ~PixelShaderSet();

	APixelShader* next();
};

class AllPixelShaders : public PixelShaderSet {
protected:
	APixelShader* createIndex(int index) {
		switch(index) {
			case 0:
				printf("[AllPixelShaders] Return NULL\n");
				return NULL;
			case 1:
				printf("[AllPixelShaders] Return SparkleShader - fast, 90%% 50-100\n");
				return new SparkleShader(15, 15, 0.9, 1.0, 0.5);
			case 2:
				printf("[AllPixelShaders] Return SparkleShader - middle speed, 100%%, 0-100\n");
				return new SparkleShader(50, 50, 1.0, 1.0, 0.0);
			case 3:
				printf("[AllPixelShaders] Return SparkleShader - middle speed, 10%%, 100-250\n");
				return new SparkleShader(20, 50, 0.1, 2.5, 1.0);
			default:
				printf("[AllPixelShaders] Return default NULL\n");
				return NULL;
		}
	}

public:
	AllPixelShaders() {
		add(0, 1.0); // NULL
//		add(1, 1.0); // SparkleShader - fast, 90%, 50-100
//		add(2, 1.0); // SparkleShader - middle speed, 100%, 0-100
//		add(3, 1.0); // SparkleShader - middle speed, 10%, 100-250
	};
};

#endif /* SRC_ANIMATION_SETS_PIXELSHADERSET_H_ */
