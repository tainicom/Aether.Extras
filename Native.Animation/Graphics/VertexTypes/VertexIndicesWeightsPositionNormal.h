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
#include "Data/Int4Data.h"
#include "Data/Vector4Data.h"
#include "Data/Vector3Data.h"

using namespace tainicom::Aether::Native::Animation::Data;


namespace tainicom
{
	namespace Aether
	{
		namespace Native
		{
			namespace Animation
			{
				namespace VertexTypes
				{
					public value struct VertexIndicesWeightsPositionNormal
					{
					public:
						Int4Data BlendIndices;
						Vector4Data BlendWeights;
						Vector3Data Position;
						Vector3Data Normal;
					};
				}
			}
		}
	}
}