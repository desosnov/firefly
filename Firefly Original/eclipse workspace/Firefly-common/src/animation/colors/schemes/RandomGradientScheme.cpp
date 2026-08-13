/*
 * RandomGradientScheme.cpp
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#include <animation/colors/schemes/RandomGradientScheme.h>

RandomGradientScheme::~RandomGradientScheme() {
	// TODO Auto-generated destructor stub
}

AColorPattern* RandomGradientScheme::nextColor() {
	return new TwoColorGradientPattern(palette->randomColor(), palette->randomColor());
}
