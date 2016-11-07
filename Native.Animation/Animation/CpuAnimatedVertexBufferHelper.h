//   Copyright 2015-2016 Kastellanos Nikolaos
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

#pragma once
#include "..\Data\Matrix.h"
#include "..\Graphics\VertexTypes\VertexIndicesWeightsPositionNormal.h" 
#include "..\VertexPositionNormalTextureData.h"

using namespace Platform;
using namespace tainicom::Aether::Native::Animation::Data;
using namespace tainicom::Aether::Native::Animation::VertexTypes;


namespace tainicom
{
	namespace Aether
	{
		namespace Native
		{
			namespace Animation
			{
				public ref class CpuAnimatedVertexBufferHelper sealed
				{
				private:
					VertexIndicesWeightsPositionNormal* _cpuVertices;
					int _cpuVerticesLength;

				public:
					CpuAnimatedVertexBufferHelper(void);
					virtual ~CpuAnimatedVertexBufferHelper(void);
					void SetCpuVertices(const Platform::Array<VertexIndicesWeightsPositionNormal>^ cpuVertices);
					void UpdateVertices(int64 pBoneTransforms, int64 pGpuVertices, int startIndex, int elementCount);

				private:
					void UpdateVertices(MatrixData* pBoneTransforms, VertexPositionNormalTextureData* pGpuVertex, int startIndex, int elementCount);

				};
			}
		}
	}
}