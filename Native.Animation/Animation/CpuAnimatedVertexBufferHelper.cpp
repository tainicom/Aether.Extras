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

#include "pch.h"
#include <malloc.h>
#include "CpuAnimatedVertexBufferHelper.h"
#include <vector>
#include <collection.h>

//using namespace DirectX;
using namespace Platform;
using namespace Platform::Collections;
using namespace tainicom::Aether::Native::Animation;
using namespace tainicom::Aether::Native::Animation::Data;


tainicom::Aether::Native::Animation::CpuAnimatedVertexBufferHelper::CpuAnimatedVertexBufferHelper(void)
{
	_cpuVertices = nullptr;
	_cpuVerticesLength = 0;
}

tainicom::Aether::Native::Animation::CpuAnimatedVertexBufferHelper::~CpuAnimatedVertexBufferHelper(void)
{
	delete _cpuVertices;
	_cpuVerticesLength = 0;
}

void tainicom::Aether::Native::Animation::CpuAnimatedVertexBufferHelper::SetCpuVertices(const Platform::Array<VertexIndicesWeightsPositionNormal>^ cpuVertices)
{
	_cpuVerticesLength = cpuVertices->Length;
	_cpuVertices = new VertexIndicesWeightsPositionNormal[_cpuVerticesLength = cpuVertices->Length];
	for (int i = 0;i<_cpuVerticesLength;i++)
	{
		_cpuVertices[i] = cpuVertices[i];
	}
	return;
}

void tainicom::Aether::Native::Animation::CpuAnimatedVertexBufferHelper::UpdateVertices(
	int64 pBoneTransforms, int64 pGpuVertices, int startIndex, int elementCount)
{
	MatrixData* pBone = (MatrixData*)pBoneTransforms;
	VertexPositionNormalTextureData* pGpuVertex = (VertexPositionNormalTextureData*)pGpuVertices;
	UpdateVertices(pBone, pGpuVertex, startIndex, elementCount);
}


void tainicom::Aether::Native::Animation::CpuAnimatedVertexBufferHelper::UpdateVertices(
	MatrixData* pBoneTransforms, VertexPositionNormalTextureData* pGpuVertices, int startIndex, int elementCount)
{
	VertexPositionNormalTextureData* vout = pGpuVertices;

	MatrixData transformSum;
	transformSum.M14 = 0;
	transformSum.M24 = 0;
	transformSum.M34 = 0;
	transformSum.M44 = 1;

	// skin all of the vertices
	int endIndex = (startIndex + elementCount - 1);
	for (int i = startIndex; i <= endIndex; i++)
	{
		int b0 = _cpuVertices[i].BlendIndices.X;
		int b1 = _cpuVertices[i].BlendIndices.Y;
		int b2 = _cpuVertices[i].BlendIndices.Z;
		int b3 = _cpuVertices[i].BlendIndices.W;

		MatrixData* m1 = &pBoneTransforms[b0];
		MatrixData* m2 = &pBoneTransforms[b1];
		MatrixData* m3 = &pBoneTransforms[b2];
		MatrixData* m4 = &pBoneTransforms[b3];

		float w1 = _cpuVertices[i].BlendWeights.X;
		float w2 = _cpuVertices[i].BlendWeights.Y;
		float w3 = _cpuVertices[i].BlendWeights.Z;
		float w4 = _cpuVertices[i].BlendWeights.W;

		transformSum.M11 = (m1->M11 * w1) + (m2->M11 * w2) + (m3->M11 * w3) + (m4->M11 * w4);
		transformSum.M12 = (m1->M12 * w1) + (m2->M12 * w2) + (m3->M12 * w3) + (m4->M12 * w4);
		transformSum.M13 = (m1->M13 * w1) + (m2->M13 * w2) + (m3->M13 * w3) + (m4->M13 * w4);
		transformSum.M21 = (m1->M21 * w1) + (m2->M21 * w2) + (m3->M21 * w3) + (m4->M21 * w4);
		transformSum.M22 = (m1->M22 * w1) + (m2->M22 * w2) + (m3->M22 * w3) + (m4->M22 * w4);
		transformSum.M23 = (m1->M23 * w1) + (m2->M23 * w2) + (m3->M23 * w3) + (m4->M23 * w4);
		transformSum.M31 = (m1->M31 * w1) + (m2->M31 * w2) + (m3->M31 * w3) + (m4->M31 * w4);
		transformSum.M32 = (m1->M32 * w1) + (m2->M32 * w2) + (m3->M32 * w3) + (m4->M32 * w4);
		transformSum.M33 = (m1->M33 * w1) + (m2->M33 * w2) + (m3->M33 * w3) + (m4->M33 * w4);
		transformSum.M41 = (m1->M41 * w1) + (m2->M41 * w2) + (m3->M41 * w3) + (m4->M41 * w4);
		transformSum.M42 = (m1->M42 * w1) + (m2->M42 * w2) + (m3->M42 * w3) + (m4->M42 * w4);
		transformSum.M43 = (m1->M43 * w1) + (m2->M43 * w2) + (m3->M43 * w3) + (m4->M43 * w4);

		// Support the 4 Bone Influences - Position then Normal
		Vector3Data position = _cpuVertices[i].Position;
		vout[i].Position.X = position.X * transformSum.M11 + position.Y * transformSum.M21 + position.Z * transformSum.M31 + transformSum.M41;
		vout[i].Position.Y = position.X * transformSum.M12 + position.Y * transformSum.M22 + position.Z * transformSum.M32 + transformSum.M42;
		vout[i].Position.Z = position.X * transformSum.M13 + position.Y * transformSum.M23 + position.Z * transformSum.M33 + transformSum.M43;
		Vector3Data normal = _cpuVertices[i].Normal;
		vout[i].Normal.X = normal.X * transformSum.M11 + normal.Y * transformSum.M21 + normal.Z * transformSum.M31;
		vout[i].Normal.Y = normal.X * transformSum.M12 + normal.Y * transformSum.M22 + normal.Z * transformSum.M32;
		vout[i].Normal.Z = normal.X * transformSum.M13 + normal.Y * transformSum.M23 + normal.Z * transformSum.M33;
	}

}