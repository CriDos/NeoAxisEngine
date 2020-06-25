// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
#pragma once

// WinRT name conflict: http://stackoverflow.com/questions/13122266/freetype-generic-conflict-with-c-cx-keyword
#define generic GenericFromFreeTypeLibrary
	#include <ft2build.h>
	#include FT_FREETYPE_H
	#include FT_STROKER_H
	#include FT_SYSTEM_H
	#include FT_MODULE_H
#undef generic

#include <vector>

//#define _DefinedMemoryAllocationType MemoryAllocationType_Other
////#define _DefinedMemoryAllocationType MemoryAllocationType_Renderer
//#include "MemoryManager.h"


#ifdef _WIN32
	#define EXPORT extern "C" __declspec(dllexport)
#else
	#define EXPORT extern "C" __attribute__ ((visibility("default")))
#endif

#ifdef _MSC_VER
#define MIN __min
#define MAX __max
#else
#define MIN std::min
#define MAX std::max
#endif

typedef unsigned char uint8;
typedef unsigned short uint16;
typedef unsigned int uint32;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct Vec2
{
	Vec2() { }
	Vec2(int a, int b) : x(a), y(b) { }

	int x, y;
};

/////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct Rect
{
	Rect() { }
	Rect(int left, int top, int right, int bottom)
		: xmin(left), xmax(right), ymin(top), ymax(bottom) { }

	void Include(const Vec2 &r)
	{
		xmin = MIN(xmin, r.x);
		ymin = MIN(ymin, r.y);
		xmax = MAX(xmax, r.x);
		ymax = MAX(ymax, r.y);
	}

	int Width() const { return xmax - xmin + 1; }
	int Height() const { return ymax - ymin + 1; }

	int xmin, xmax, ymin, ymax;
};

/////////////////////////////////////////////////////////////////////////////////////////////////////////////

// A horizontal pixel span generated by the FreeType renderer.
struct Span
{
	Span() { }
	Span(int _x, int _y, int _width, int _coverage)
		: x(_x), y(_y), width(_width), coverage(_coverage) { }

	int x, y, width, coverage;
};

typedef std::vector<Span> Spans;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////

void* My_Alloc_Func(FT_Memory memory, long size)
{
	return malloc(size);
}

void My_Free_Func(FT_Memory memory, void *block)
{
	free(block);
}

void* My_Realloc_Func(FT_Memory memory, long cur_size, long new_size, void* block)
{
	return realloc(block, new_size);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////

//extern FT_Memory ftMemory;
//extern int memoryCounter;

EXPORT FT_Library FreeType_Init()
{
	//// Set up the memory management.
	//if(!ftMemory)
	//{
	//	ftMemory = FT_New_Memory();// = new FT_MemoryRec_;
	//	ftMemory->user = NULL;
	//	ftMemory->alloc = My_Alloc_Func;
	//	ftMemory->free = My_Free_Func;
	//	ftMemory->realloc = My_Realloc_Func;
	//}
	//memoryCounter++;

	FT_Library library;
	//if( FT_New_Library( ftMemory, &library ) )
	//	return NULL;
	if( FT_Init_FreeType( &library ) )
		return NULL;

	return library;
}

EXPORT void FreeType_Shutdown(FT_Library library)
{
	FT_Done_FreeType( library );

	//memoryCounter--;
	//if(memoryCounter <= 0)
	//{
	//	if(ftMemory)
	//	{
	//		delete ftMemory;
	//		ftMemory = NULL;
	//	}
	//}

}

EXPORT FT_Face FreeType_CreateFace(FT_Library library, FT_Byte* ttfData, int ttfDataSize)
{
	FT_Face face;

	FT_Error error;

	error = FT_New_Memory_Face( library, ttfData, ttfDataSize, 0, &face );
	if( error == FT_Err_Unknown_File_Format)
		return NULL;
	else if( error )
		return NULL;

	//FT_Select_Charmap( face, FT_ENCODING_UNICODE );

	return face;
}

EXPORT void FreeType_DestroyFace(FT_Face face)
{
	FT_Done_Face(face);
}

EXPORT bool FreeType_SetPixelSizes(FT_Face face, int sizeX, int sizeY)
{
	if(FT_Set_Pixel_Sizes( face, sizeX, sizeY ))
		return false;
	return true;
}

void RasterCallback(const int y, const int count, const FT_Span* const spans, void* const user) 
{
	Spans* sptr = (Spans*)user;
	for (int i = 0; i < count; ++i) 
		sptr->push_back(Span(spans[i].x, y, spans[i].len, spans[i].coverage));
}

void RenderSpans(FT_Library& library, FT_Outline* const outline, Spans* spans) 
{
	FT_Raster_Params params;
	memset(&params, 0, sizeof(params));
	params.flags = FT_RASTER_FLAG_AA | FT_RASTER_FLAG_DIRECT;
	params.gray_spans = RasterCallback;
	params.user = spans;

	FT_Outline_Render(library, outline, &params);
}

EXPORT bool FreeType_IsGlyphExists( FT_Library library, FT_Face face, int character)
{
	FT_UInt glyph_index = FT_Get_Char_Index( face, character );

	FT_Error ftResult;

	ftResult = FT_Load_Glyph( face, glyph_index, FT_LOAD_NO_BITMAP );
	if(ftResult)
		return false;

	// Need an outline for this to work.
	if(face->glyph->format != FT_GLYPH_FORMAT_OUTLINE)
		return false;

	FT_Glyph glyph;
	if (FT_Get_Glyph(face->glyph, &glyph) != 0)
		return false;

	return true;
}

EXPORT bool FreeType_GetGlyphData( FT_Library library, FT_Face face, int character, int bufferSizeX, 
	int bufferSizeY, uint8* buffer, int* drawOffsetX, int* drawOffsetY, int* outSizeX, int* outSizeY, 
	int* advance)
{
	*outSizeX = 0;
	*outSizeY = 0;
	*drawOffsetX = 0;
	*drawOffsetY = 0;
	*advance = 0;

	FT_UInt glyph_index = FT_Get_Char_Index( face, character );

	FT_Error ftResult;

	ftResult = FT_Load_Glyph( face, glyph_index, FT_LOAD_NO_BITMAP );
	if(ftResult)
		return false;

	// Need an outline for this to work.
	if(face->glyph->format != FT_GLYPH_FORMAT_OUTLINE)
		return false;

	// Render the basic glyph to a span list.
	Spans spans;
	RenderSpans(library, &face->glyph->outline, &spans);

	FT_Glyph glyph;
	if (FT_Get_Glyph(face->glyph, &glyph) != 0)
		return false;

	FT_Done_Glyph(glyph);

	*drawOffsetX = face->glyph->metrics.horiBearingX >> 6;
	*drawOffsetY = face->glyph->metrics.horiBearingY >> 6;
	*advance = face->glyph->metrics.horiAdvance >> 6;

	//clear buffer
	memset(buffer, 0, bufferSizeX * bufferSizeY);

	if(!spans.empty())
	{
		// Figure out what the bounding rect is for both the span lists.
		Rect rect(spans.front().x, spans.front().y, spans.front().x, spans.front().y);
		for(Spans::iterator s = spans.begin(); s != spans.end(); ++s)
		{
			rect.Include(Vec2(s->x, s->y));
			rect.Include(Vec2(s->x + s->width - 1, s->y));
		}

		int sizeX = rect.Width();
		int sizeY = rect.Height();

		*outSizeX = sizeX;
		*outSizeY = sizeY;

		if(buffer)
		{
			// Then loop over the regular glyph spans and blend them into the image.
			for(Spans::iterator s = spans.begin(); s != spans.end(); ++s)
			{
				for(int w = 0; w < s->width; ++w)
				{
					int y = sizeY - 1 - (s->y - rect.ymin);
					int x = s->x - rect.xmin + w;

					if( x < bufferSizeX && y < bufferSizeY )
					{
						uint8* pointer = buffer + (y * bufferSizeY + x);
						*pointer = MIN(*pointer + s->coverage, 255);
					}
				}
			}
		}

	}

	return true;
}