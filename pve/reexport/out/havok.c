#include "callback.h"
#include <stdio.h>

struct Vector3 {
	float X;
	float Y;
	float Z;
};

struct Quaternion {
	float X;
	float Y;
	float Z;
	float W;
};

struct Matrix {
	float M11;
	float M12;
	float M13;
	float M14;
	float M21;
	float M22;
	float M23;
	float M24;
	float M31;
	float M32;
	float M33;
	float M34;
	float M41;
	float M42;
	float M43;
	float M44;
};

struct Vector4 {
	float X;
	float Y;
	float Z;
	float W;
};

struct SplittingData {
	struct Vector4 SplittingAxis;
	float NumSubparts;
	float WidthRange;
	struct Vector4 Scale;
	struct Vector4 ScaleRange;
	float SplitGeomShiftRangeY;
	float SplitGeomShiftRangeZ;
	float SurfaceNormalShearingRange;
	float FractureLineShearingRange;
	float FractureNormalShearingRange;
	int m_rotateSplitGeom;
};

struct HkMassProperties {
	float Volume;
	float Mass;
	struct Vector3 CenterOfMass;
	struct Matrix InertiaTensor;
};

struct Vector2 {
	float X;
	float Y;
};

struct Vector3I {
	int X;
	int Y;
	int Z;
};

struct Vector3S {
	short X;
	short Y;
	short Z;
};

struct DecomposeShapeKeyResult {
	int instanceId;
	int childKey;
};

struct HkUniformGridShapeArgsPOD {
	int CellsCount_X;
	int CellsCount_Y;
	int CellsCount_Z;
	float CellSize;
	float CellOffset;
	float CellExpand;
};

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkConstraint_ReadConstraintsCallback(void * constraintList, int constraintCount, void * userData) {
	printf("callback Havok_HkConstraint_ReadConstraintsCallback\n");
	typedef void (*callback_ptr_t)(void * constraintList, int constraintCount, void * userData);
	return ((callback_ptr_t)cb_userdata_tls)(constraintList, constraintCount, userData);
}
void * _PVE_Trampoline_Havok_HkConstraint_ReadConstraintsCallback(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkConstraint_ReadConstraintsCallback, ptr);
	printf("set callback Havok_HkConstraint_ReadConstraintsCallback - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkConstraintListener_OnAdded(void * listener) {
	printf("callback Havok_HkConstraintListener_OnAdded\n");
	typedef void (*callback_ptr_t)(void * listener);
	return ((callback_ptr_t)cb_userdata_tls)(listener);
}
void * _PVE_Trampoline_Havok_HkConstraintListener_OnAdded(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkConstraintListener_OnAdded, ptr);
	printf("set callback Havok_HkConstraintListener_OnAdded - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkConstraintListener_OnRemoved(void * listener) {
	printf("callback Havok_HkConstraintListener_OnRemoved\n");
	typedef void (*callback_ptr_t)(void * listener);
	return ((callback_ptr_t)cb_userdata_tls)(listener);
}
void * _PVE_Trampoline_Havok_HkConstraintListener_OnRemoved(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkConstraintListener_OnRemoved, ptr);
	printf("set callback Havok_HkConstraintListener_OnRemoved - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkConstraintListener_OnBreaking(void * listener) {
	printf("callback Havok_HkConstraintListener_OnBreaking\n");
	typedef void (*callback_ptr_t)(void * listener);
	return ((callback_ptr_t)cb_userdata_tls)(listener);
}
void * _PVE_Trampoline_Havok_HkConstraintListener_OnBreaking(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkConstraintListener_OnBreaking, ptr);
	printf("set callback Havok_HkConstraintListener_OnBreaking - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

int __attribute__((ms_abi)) _PVE_Stub_Havok_HkBreakOffPartsUtil_BreakLogicHandlerDelegate(void * body, void * otherBody, int shapeKey, void * maxImpulse) {
	printf("callback Havok_HkBreakOffPartsUtil_BreakLogicHandlerDelegate\n");
	typedef int (*callback_ptr_t)(void * body, void * otherBody, int shapeKey, void * maxImpulse);
	return ((callback_ptr_t)cb_userdata_tls)(body, otherBody, shapeKey, maxImpulse);
}
void * _PVE_Trampoline_Havok_HkBreakOffPartsUtil_BreakLogicHandlerDelegate(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkBreakOffPartsUtil_BreakLogicHandlerDelegate, ptr);
	printf("set callback Havok_HkBreakOffPartsUtil_BreakLogicHandlerDelegate - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

int __attribute__((ms_abi)) _PVE_Stub_Havok_HkBreakOffPartsUtil_BreakPartsHandlerDelegate(void * body, void * breakOffPoints) {
	printf("callback Havok_HkBreakOffPartsUtil_BreakPartsHandlerDelegate\n");
	typedef int (*callback_ptr_t)(void * body, void * breakOffPoints);
	return ((callback_ptr_t)cb_userdata_tls)(body, breakOffPoints);
}
void * _PVE_Trampoline_Havok_HkBreakOffPartsUtil_BreakPartsHandlerDelegate(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkBreakOffPartsUtil_BreakPartsHandlerDelegate, ptr);
	printf("set callback Havok_HkBreakOffPartsUtil_BreakPartsHandlerDelegate - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkdBreakableBody_CallBodyReplacedEvent(void * replaceBodyEvent) {
	printf("callback Havok_HkdBreakableBody_CallBodyReplacedEvent\n");
	typedef void (*callback_ptr_t)(void * replaceBodyEvent);
	return ((callback_ptr_t)cb_userdata_tls)(replaceBodyEvent);
}
void * _PVE_Trampoline_Havok_HkdBreakableBody_CallBodyReplacedEvent(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkdBreakableBody_CallBodyReplacedEvent, ptr);
	printf("set callback Havok_HkdBreakableBody_CallBodyReplacedEvent - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkdBreakableBody_CallBreakableBodyEvent(void * breakableBody) {
	printf("callback Havok_HkdBreakableBody_CallBreakableBodyEvent\n");
	typedef void (*callback_ptr_t)(void * breakableBody);
	return ((callback_ptr_t)cb_userdata_tls)(breakableBody);
}
void * _PVE_Trampoline_Havok_HkdBreakableBody_CallBreakableBodyEvent(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkdBreakableBody_CallBreakableBodyEvent, ptr);
	printf("set callback Havok_HkdBreakableBody_CallBreakableBodyEvent - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkdBreakableBodyHelper_ReturnShapeInstanceInfo(void * shapeInstanceInfo) {
	printf("callback Havok_HkdBreakableBodyHelper_ReturnShapeInstanceInfo\n");
	typedef void (*callback_ptr_t)(void * shapeInstanceInfo);
	return ((callback_ptr_t)cb_userdata_tls)(shapeInstanceInfo);
}
void * _PVE_Trampoline_Havok_HkdBreakableBodyHelper_ReturnShapeInstanceInfo(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkdBreakableBodyHelper_ReturnShapeInstanceInfo, ptr);
	printf("set callback Havok_HkdBreakableBodyHelper_ReturnShapeInstanceInfo - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkdBreakableShape_ReturnShapeInstanceInfo(void * shapeInstanceInfo) {
	printf("callback Havok_HkdBreakableShape_ReturnShapeInstanceInfo\n");
	typedef void (*callback_ptr_t)(void * shapeInstanceInfo);
	return ((callback_ptr_t)cb_userdata_tls)(shapeInstanceInfo);
}
void * _PVE_Trampoline_Havok_HkdBreakableShape_ReturnShapeInstanceInfo(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkdBreakableShape_ReturnShapeInstanceInfo, ptr);
	printf("set callback Havok_HkdBreakableShape_ReturnShapeInstanceInfo - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkdBreakableShape_ReturnConnection(void * connection) {
	printf("callback Havok_HkdBreakableShape_ReturnConnection\n");
	typedef void (*callback_ptr_t)(void * connection);
	return ((callback_ptr_t)cb_userdata_tls)(connection);
}
void * _PVE_Trampoline_Havok_HkdBreakableShape_ReturnConnection(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkdBreakableShape_ReturnConnection, ptr);
	printf("set callback Havok_HkdBreakableShape_ReturnConnection - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkdReplaceBodyEvent_ReturnBreakableBodyInfo(void * breakableBodyInfo) {
	printf("callback Havok_HkdReplaceBodyEvent_ReturnBreakableBodyInfo\n");
	typedef void (*callback_ptr_t)(void * breakableBodyInfo);
	return ((callback_ptr_t)cb_userdata_tls)(breakableBodyInfo);
}
void * _PVE_Trampoline_Havok_HkdReplaceBodyEvent_ReturnBreakableBodyInfo(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkdReplaceBodyEvent_ReturnBreakableBodyInfo, ptr);
	printf("set callback Havok_HkdReplaceBodyEvent_ReturnBreakableBodyInfo - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkdShapeInstanceInfo_ReturnShapeInstanceInfo(void * shapeInstanceInfo) {
	printf("callback Havok_HkdShapeInstanceInfo_ReturnShapeInstanceInfo\n");
	typedef void (*callback_ptr_t)(void * shapeInstanceInfo);
	return ((callback_ptr_t)cb_userdata_tls)(shapeInstanceInfo);
}
void * _PVE_Trampoline_Havok_HkdShapeInstanceInfo_ReturnShapeInstanceInfo(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkdShapeInstanceInfo_ReturnShapeInstanceInfo, ptr);
	printf("set callback Havok_HkdShapeInstanceInfo_ReturnShapeInstanceInfo - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkDestructionStorage_ReturnSectionData(int indexStart, int triCount, void * materialName) {
	printf("callback Havok_HkDestructionStorage_ReturnSectionData\n");
	typedef void (*callback_ptr_t)(int indexStart, int triCount, void * materialName);
	return ((callback_ptr_t)cb_userdata_tls)(indexStart, triCount, materialName);
}
void * _PVE_Trampoline_Havok_HkDestructionStorage_ReturnSectionData(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkDestructionStorage_ReturnSectionData, ptr);
	printf("set callback Havok_HkDestructionStorage_ReturnSectionData - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkDestructionStorage_ReturnIndex(int index) {
	printf("callback Havok_HkDestructionStorage_ReturnIndex\n");
	typedef void (*callback_ptr_t)(int index);
	return ((callback_ptr_t)cb_userdata_tls)(index);
}
void * _PVE_Trampoline_Havok_HkDestructionStorage_ReturnIndex(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkDestructionStorage_ReturnIndex, ptr);
	printf("set callback Havok_HkDestructionStorage_ReturnIndex - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkDestructionStorage_ReturnVertex(struct Vector3 position, struct Vector3 normal, struct Vector3 tangent, struct Vector2 texCoord) {
	printf("callback Havok_HkDestructionStorage_ReturnVertex\n");
	typedef void (*callback_ptr_t)(struct Vector3 position, struct Vector3 normal, struct Vector3 tangent, struct Vector2 texCoord);
	return ((callback_ptr_t)cb_userdata_tls)(position, normal, tangent, texCoord);
}
void * _PVE_Trampoline_Havok_HkDestructionStorage_ReturnVertex(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkDestructionStorage_ReturnVertex, ptr);
	printf("set callback Havok_HkDestructionStorage_ReturnVertex - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkDestructionStorage_ReturnString(void * value) {
	printf("callback Havok_HkDestructionStorage_ReturnString\n");
	typedef void (*callback_ptr_t)(void * value);
	return ((callback_ptr_t)cb_userdata_tls)(value);
}
void * _PVE_Trampoline_Havok_HkDestructionStorage_ReturnString(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkDestructionStorage_ReturnString, ptr);
	printf("set callback Havok_HkDestructionStorage_ReturnString - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkDestructionStorage_ReturnBreakableShape(void * shape) {
	printf("callback Havok_HkDestructionStorage_ReturnBreakableShape\n");
	typedef void (*callback_ptr_t)(void * shape);
	return ((callback_ptr_t)cb_userdata_tls)(shape);
}
void * _PVE_Trampoline_Havok_HkDestructionStorage_ReturnBreakableShape(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkDestructionStorage_ReturnBreakableShape, ptr);
	printf("set callback Havok_HkDestructionStorage_ReturnBreakableShape - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkDestructionStorage_ReturnByteArray(void * byteArray, int size) {
	printf("callback Havok_HkDestructionStorage_ReturnByteArray\n");
	typedef void (*callback_ptr_t)(void * byteArray, int size);
	return ((callback_ptr_t)cb_userdata_tls)(byteArray, size);
}
void * _PVE_Trampoline_Havok_HkDestructionStorage_ReturnByteArray(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkDestructionStorage_ReturnByteArray, ptr);
	printf("set callback Havok_HkDestructionStorage_ReturnByteArray - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

float __attribute__((ms_abi)) _PVE_Stub_Havok_HkWheelResponseModifierUtil_CalculateModifier(void * handle) {
	printf("callback Havok_HkWheelResponseModifierUtil_CalculateModifier\n");
	typedef float (*callback_ptr_t)(void * handle);
	return ((callback_ptr_t)cb_userdata_tls)(handle);
}
void * _PVE_Trampoline_Havok_HkWheelResponseModifierUtil_CalculateModifier(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkWheelResponseModifierUtil_CalculateModifier, ptr);
	printf("set callback Havok_HkWheelResponseModifierUtil_CalculateModifier - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkActivationListener_HkActivationHandlerCpp(void * handler) {
	printf("callback Havok_HkActivationListener_HkActivationHandlerCpp\n");
	typedef void (*callback_ptr_t)(void * handler);
	return ((callback_ptr_t)cb_userdata_tls)(handler);
}
void * _PVE_Trampoline_Havok_HkActivationListener_HkActivationHandlerCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkActivationListener_HkActivationHandlerCpp, ptr);
	printf("set callback Havok_HkActivationListener_HkActivationHandlerCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkBaseSystem_Log(void * message) {
	printf("callback Havok_HkBaseSystem_Log\n");
	typedef void (*callback_ptr_t)(void * message);
	return ((callback_ptr_t)cb_userdata_tls)(message);
}
void * _PVE_Trampoline_Havok_HkBaseSystem_Log(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkBaseSystem_Log, ptr);
	printf("set callback Havok_HkBaseSystem_Log - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkContactListener_ContactPointHandler(void * handle, void * contactPointEvent) {
	printf("callback Havok_HkContactListener_ContactPointHandler\n");
	typedef void (*callback_ptr_t)(void * handle, void * contactPointEvent);
	return ((callback_ptr_t)cb_userdata_tls)(handle, contactPointEvent);
}
void * _PVE_Trampoline_Havok_HkContactListener_ContactPointHandler(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkContactListener_ContactPointHandler, ptr);
	printf("set callback Havok_HkContactListener_ContactPointHandler - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkContactListener_CollisionHandler(void * handle, void * collisionEvent) {
	printf("callback Havok_HkContactListener_CollisionHandler\n");
	typedef void (*callback_ptr_t)(void * handle, void * collisionEvent);
	return ((callback_ptr_t)cb_userdata_tls)(handle, collisionEvent);
}
void * _PVE_Trampoline_Havok_HkContactListener_CollisionHandler(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkContactListener_CollisionHandler, ptr);
	printf("set callback Havok_HkContactListener_CollisionHandler - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkContactSoundListener_ContactSoundHandler(void * handle, void * contactPointEvent) {
	printf("callback Havok_HkContactSoundListener_ContactSoundHandler\n");
	typedef void (*callback_ptr_t)(void * handle, void * contactPointEvent);
	return ((callback_ptr_t)cb_userdata_tls)(handle, contactPointEvent);
}
void * _PVE_Trampoline_Havok_HkContactSoundListener_ContactSoundHandler(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkContactSoundListener_ContactSoundHandler, ptr);
	printf("set callback Havok_HkContactSoundListener_ContactSoundHandler - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkEntityListener_OnAddCpp(void * listener, void * entity) {
	printf("callback Havok_HkEntityListener_OnAddCpp\n");
	typedef void (*callback_ptr_t)(void * listener, void * entity);
	return ((callback_ptr_t)cb_userdata_tls)(listener, entity);
}
void * _PVE_Trampoline_Havok_HkEntityListener_OnAddCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkEntityListener_OnAddCpp, ptr);
	printf("set callback Havok_HkEntityListener_OnAddCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkEntityListener_OnRemoveCpp(void * listener, void * entity) {
	printf("callback Havok_HkEntityListener_OnRemoveCpp\n");
	typedef void (*callback_ptr_t)(void * listener, void * entity);
	return ((callback_ptr_t)cb_userdata_tls)(listener, entity);
}
void * _PVE_Trampoline_Havok_HkEntityListener_OnRemoveCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkEntityListener_OnRemoveCpp, ptr);
	printf("set callback Havok_HkEntityListener_OnRemoveCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkEntityListener_OnDeleteCpp(void * listener, void * entity) {
	printf("callback Havok_HkEntityListener_OnDeleteCpp\n");
	typedef void (*callback_ptr_t)(void * listener, void * entity);
	return ((callback_ptr_t)cb_userdata_tls)(listener, entity);
}
void * _PVE_Trampoline_Havok_HkEntityListener_OnDeleteCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkEntityListener_OnDeleteCpp, ptr);
	printf("set callback Havok_HkEntityListener_OnDeleteCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkEntityListener_OnShapeChangeCpp(void * listener, void * entity) {
	printf("callback Havok_HkEntityListener_OnShapeChangeCpp\n");
	typedef void (*callback_ptr_t)(void * listener, void * entity);
	return ((callback_ptr_t)cb_userdata_tls)(listener, entity);
}
void * _PVE_Trampoline_Havok_HkEntityListener_OnShapeChangeCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkEntityListener_OnShapeChangeCpp, ptr);
	printf("set callback Havok_HkEntityListener_OnShapeChangeCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkEntityListener_OnMotionTypeChangeCpp(void * listener, void * entity) {
	printf("callback Havok_HkEntityListener_OnMotionTypeChangeCpp\n");
	typedef void (*callback_ptr_t)(void * listener, void * entity);
	return ((callback_ptr_t)cb_userdata_tls)(listener, entity);
}
void * _PVE_Trampoline_Havok_HkEntityListener_OnMotionTypeChangeCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkEntityListener_OnMotionTypeChangeCpp, ptr);
	printf("set callback Havok_HkEntityListener_OnMotionTypeChangeCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkJobThreadPool_ThreadAction(void * data) {
	printf("callback Havok_HkJobThreadPool_ThreadAction\n");
	typedef void (*callback_ptr_t)(void * data);
	return ((callback_ptr_t)cb_userdata_tls)(data);
}
void * _PVE_Trampoline_Havok_HkJobThreadPool_ThreadAction(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkJobThreadPool_ThreadAction, ptr);
	printf("set callback Havok_HkJobThreadPool_ThreadAction - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkTaskProfiler_TaskStartedFuncCpp(void * name, int type) {
	printf("callback Havok_HkTaskProfiler_TaskStartedFuncCpp\n");
	typedef void (*callback_ptr_t)(void * name, int type);
	return ((callback_ptr_t)cb_userdata_tls)(name, type);
}
void * _PVE_Trampoline_Havok_HkTaskProfiler_TaskStartedFuncCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkTaskProfiler_TaskStartedFuncCpp, ptr);
	printf("set callback Havok_HkTaskProfiler_TaskStartedFuncCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkTaskProfiler_TaskFinishedFunc() {
	printf("callback Havok_HkTaskProfiler_TaskFinishedFunc\n");
	typedef void (*callback_ptr_t)();
	return ((callback_ptr_t)cb_userdata_tls)();
}
void * _PVE_Trampoline_Havok_HkTaskProfiler_TaskFinishedFunc(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkTaskProfiler_TaskFinishedFunc, ptr);
	printf("set callback Havok_HkTaskProfiler_TaskFinishedFunc - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkTaskProfiler_BlockBeginFuncCpp(void * name) {
	printf("callback Havok_HkTaskProfiler_BlockBeginFuncCpp\n");
	typedef void (*callback_ptr_t)(void * name);
	return ((callback_ptr_t)cb_userdata_tls)(name);
}
void * _PVE_Trampoline_Havok_HkTaskProfiler_BlockBeginFuncCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkTaskProfiler_BlockBeginFuncCpp, ptr);
	printf("set callback Havok_HkTaskProfiler_BlockBeginFuncCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkTaskProfiler_BlockEndFunc(long int ticks) {
	printf("callback Havok_HkTaskProfiler_BlockEndFunc\n");
	typedef void (*callback_ptr_t)(long int ticks);
	return ((callback_ptr_t)cb_userdata_tls)(ticks);
}
void * _PVE_Trampoline_Havok_HkTaskProfiler_BlockEndFunc(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkTaskProfiler_BlockEndFunc, ptr);
	printf("set callback Havok_HkTaskProfiler_BlockEndFunc - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkWorld_BroadPhaseExitCallback(void * world, void * body) {
	printf("callback Havok_HkWorld_BroadPhaseExitCallback\n");
	typedef void (*callback_ptr_t)(void * world, void * body);
	return ((callback_ptr_t)cb_userdata_tls)(world, body);
}
void * _PVE_Trampoline_Havok_HkWorld_BroadPhaseExitCallback(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkWorld_BroadPhaseExitCallback, ptr);
	printf("set callback Havok_HkWorld_BroadPhaseExitCallback - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkpAabbPhantom_CollidableAddedD(void * phantomHandle, void * e) {
	printf("callback Havok_HkpAabbPhantom_CollidableAddedD\n");
	typedef void (*callback_ptr_t)(void * phantomHandle, void * e);
	return ((callback_ptr_t)cb_userdata_tls)(phantomHandle, e);
}
void * _PVE_Trampoline_Havok_HkpAabbPhantom_CollidableAddedD(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkpAabbPhantom_CollidableAddedD, ptr);
	printf("set callback Havok_HkpAabbPhantom_CollidableAddedD - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkpAabbPhantom_CollidableRemovedD(void * phantomHandle, void * e) {
	printf("callback Havok_HkpAabbPhantom_CollidableRemovedD\n");
	typedef void (*callback_ptr_t)(void * phantomHandle, void * e);
	return ((callback_ptr_t)cb_userdata_tls)(phantomHandle, e);
}
void * _PVE_Trampoline_Havok_HkpAabbPhantom_CollidableRemovedD(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkpAabbPhantom_CollidableRemovedD, ptr);
	printf("set callback Havok_HkpAabbPhantom_CollidableRemovedD - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkPhantomCallbackShape_HkPhantomHandlerCpp(void * shape, void * body) {
	printf("callback Havok_HkPhantomCallbackShape_HkPhantomHandlerCpp\n");
	typedef void (*callback_ptr_t)(void * shape, void * body);
	return ((callback_ptr_t)cb_userdata_tls)(shape, body);
}
void * _PVE_Trampoline_Havok_HkPhantomCallbackShape_HkPhantomHandlerCpp(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkPhantomCallbackShape_HkPhantomHandlerCpp, ptr);
	printf("set callback Havok_HkPhantomCallbackShape_HkPhantomHandlerCpp - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkDeleteHandler(void * nativeObject) {
	printf("callback Havok_HkDeleteHandler\n");
	typedef void (*callback_ptr_t)(void * nativeObject);
	return ((callback_ptr_t)cb_userdata_tls)(nativeObject);
}
void * _PVE_Trampoline_Havok_HkDeleteHandler(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkDeleteHandler, ptr);
	printf("set callback Havok_HkDeleteHandler - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkShapeLoader_ReturnByteArray(void * byteArray, int size) {
	printf("callback Havok_HkShapeLoader_ReturnByteArray\n");
	typedef void (*callback_ptr_t)(void * byteArray, int size);
	return ((callback_ptr_t)cb_userdata_tls)(byteArray, size);
}
void * _PVE_Trampoline_Havok_HkShapeLoader_ReturnByteArray(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkShapeLoader_ReturnByteArray, ptr);
	printf("set callback Havok_HkShapeLoader_ReturnByteArray - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkUniformGridShape_NativeBatchRequestCallback(void * instance, int batchId) {
	printf("callback Havok_HkUniformGridShape_NativeBatchRequestCallback - 0x%016lx\n", (unsigned long)cb_userdata_tls);
	typedef void (*callback_ptr_t)(void * instance, int batchId);
	return ((callback_ptr_t)cb_userdata_tls)(instance, batchId);
}
void * _PVE_Trampoline_Havok_HkUniformGridShape_NativeBatchRequestCallback(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkUniformGridShape_NativeBatchRequestCallback, ptr);
	printf("set callback Havok_HkUniformGridShape_NativeBatchRequestCallback - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}

void __attribute__((ms_abi)) _PVE_Stub_Havok_HkDestructionUtils_ReturnBreakableShape(void * shape) {
	printf("callback Havok_HkDestructionUtils_ReturnBreakableShape\n");
	typedef void (*callback_ptr_t)(void * shape);
	return ((callback_ptr_t)cb_userdata_tls)(shape);
}
void * _PVE_Trampoline_Havok_HkDestructionUtils_ReturnBreakableShape(void * ptr) {
	void* trampoline = callback_make_trampoline(&_PVE_Stub_Havok_HkDestructionUtils_ReturnBreakableShape, ptr);
	printf("set callback Havok_HkDestructionUtils_ReturnBreakableShape - 0x%016lx\n", (unsigned long)trampoline);
	return trampoline;
}


void * (*__PVE_HkCharacterProxy_Create)(void * info) __attribute__((ms_abi));
void * HkCharacterProxy_Create(void * info) {
	printf("invoke HkCharacterProxy_Create\n");
	return __PVE_HkCharacterProxy_Create(info);
}

struct Vector3 (*__PVE_HkCharacterProxy_GetPosition)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterProxy_GetPosition(void * instance) {
	printf("invoke HkCharacterProxy_GetPosition\n");
	return __PVE_HkCharacterProxy_GetPosition(instance);
}

void (*__PVE_HkCharacterProxy_SetPosition)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterProxy_SetPosition(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterProxy_SetPosition\n");
	return __PVE_HkCharacterProxy_SetPosition(instance, value);
}

int (*__PVE_HkCharacterProxy_GetState)(void * instance) __attribute__((ms_abi));
int HkCharacterProxy_GetState(void * instance) {
	printf("invoke HkCharacterProxy_GetState\n");
	return __PVE_HkCharacterProxy_GetState(instance);
}

void (*__PVE_HkCharacterProxy_SetState)(void * instance, int value) __attribute__((ms_abi));
void HkCharacterProxy_SetState(void * instance, int value) {
	printf("invoke HkCharacterProxy_SetState\n");
	return __PVE_HkCharacterProxy_SetState(instance, value);
}

void (*__PVE_HkCharacterProxy_StepSimulation)(void * instance, float timeInSec, float posX, float posY, int jump, int wantJump, int atLadder, struct Vector3 gravity, struct Vector3 up, struct Vector3 forward) __attribute__((ms_abi));
void HkCharacterProxy_StepSimulation(void * instance, float timeInSec, float posX, float posY, int jump, int wantJump, int atLadder, struct Vector3 gravity, struct Vector3 up, struct Vector3 forward) {
	printf("invoke HkCharacterProxy_StepSimulation\n");
	return __PVE_HkCharacterProxy_StepSimulation(instance, timeInSec, posX, posY, jump, wantJump, atLadder, gravity, up, forward);
}

struct Vector3 (*__PVE_HkCharacterProxy_GetLinearVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterProxy_GetLinearVelocity(void * instance) {
	printf("invoke HkCharacterProxy_GetLinearVelocity\n");
	return __PVE_HkCharacterProxy_GetLinearVelocity(instance);
}

void (*__PVE_HkCharacterProxy_SetLinearVelocity)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterProxy_SetLinearVelocity(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterProxy_SetLinearVelocity\n");
	return __PVE_HkCharacterProxy_SetLinearVelocity(instance, value);
}

void (*__PVE_HkCharacterProxy_SetUp)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterProxy_SetUp(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterProxy_SetUp\n");
	return __PVE_HkCharacterProxy_SetUp(instance, value);
}

void * (*__PVE_HkCharacterProxyCinfo_Create)() __attribute__((ms_abi));
void * HkCharacterProxyCinfo_Create() {
	printf("invoke HkCharacterProxyCinfo_Create\n");
	return __PVE_HkCharacterProxyCinfo_Create();
}

struct Vector3 (*__PVE_HkCharacterProxyCinfo_GetPosition)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterProxyCinfo_GetPosition(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetPosition\n");
	return __PVE_HkCharacterProxyCinfo_GetPosition(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetPosition)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetPosition(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterProxyCinfo_SetPosition\n");
	return __PVE_HkCharacterProxyCinfo_SetPosition(instance, value);
}

struct Vector3 (*__PVE_HkCharacterProxyCinfo_GetVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterProxyCinfo_GetVelocity(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetVelocity\n");
	return __PVE_HkCharacterProxyCinfo_GetVelocity(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetVelocity)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetVelocity(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterProxyCinfo_SetVelocity\n");
	return __PVE_HkCharacterProxyCinfo_SetVelocity(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetDynamicFriction)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetDynamicFriction(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetDynamicFriction\n");
	return __PVE_HkCharacterProxyCinfo_GetDynamicFriction(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetDynamicFriction)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetDynamicFriction(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetDynamicFriction\n");
	return __PVE_HkCharacterProxyCinfo_SetDynamicFriction(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetStaticFriction)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetStaticFriction(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetStaticFriction\n");
	return __PVE_HkCharacterProxyCinfo_GetStaticFriction(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetStaticFriction)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetStaticFriction(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetStaticFriction\n");
	return __PVE_HkCharacterProxyCinfo_SetStaticFriction(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetKeepContactTolerance)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetKeepContactTolerance(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetKeepContactTolerance\n");
	return __PVE_HkCharacterProxyCinfo_GetKeepContactTolerance(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetKeepContactTolerance)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetKeepContactTolerance(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetKeepContactTolerance\n");
	return __PVE_HkCharacterProxyCinfo_SetKeepContactTolerance(instance, value);
}

struct Vector3 (*__PVE_HkCharacterProxyCinfo_GetUp)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterProxyCinfo_GetUp(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetUp\n");
	return __PVE_HkCharacterProxyCinfo_GetUp(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetUp)(void * instance, struct Vector3 up) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetUp(void * instance, struct Vector3 up) {
	printf("invoke HkCharacterProxyCinfo_SetUp\n");
	return __PVE_HkCharacterProxyCinfo_SetUp(instance, up);
}

float (*__PVE_HkCharacterProxyCinfo_GetExtraUpStaticFriction)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetExtraUpStaticFriction(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetExtraUpStaticFriction\n");
	return __PVE_HkCharacterProxyCinfo_GetExtraUpStaticFriction(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetExtraUpStaticFriction)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetExtraUpStaticFriction(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetExtraUpStaticFriction\n");
	return __PVE_HkCharacterProxyCinfo_SetExtraUpStaticFriction(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetExtraDownStaticFriction)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetExtraDownStaticFriction(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetExtraDownStaticFriction\n");
	return __PVE_HkCharacterProxyCinfo_GetExtraDownStaticFriction(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetExtraDownStaticFriction)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetExtraDownStaticFriction(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetExtraDownStaticFriction\n");
	return __PVE_HkCharacterProxyCinfo_SetExtraDownStaticFriction(instance, value);
}

void (*__PVE_HkCharacterProxyCinfo_SetShapePhantom)(void * instance, void * value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetShapePhantom(void * instance, void * value) {
	printf("invoke HkCharacterProxyCinfo_SetShapePhantom\n");
	return __PVE_HkCharacterProxyCinfo_SetShapePhantom(instance, value);
}

void * (*__PVE_HkCharacterProxyCinfo_GetShapePhantom)(void * instance) __attribute__((ms_abi));
void * HkCharacterProxyCinfo_GetShapePhantom(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetShapePhantom\n");
	return __PVE_HkCharacterProxyCinfo_GetShapePhantom(instance);
}

float (*__PVE_HkCharacterProxyCinfo_GetKeepDistance)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetKeepDistance(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetKeepDistance\n");
	return __PVE_HkCharacterProxyCinfo_GetKeepDistance(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetKeepDistance)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetKeepDistance(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetKeepDistance\n");
	return __PVE_HkCharacterProxyCinfo_SetKeepDistance(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetContactAngleSensitivity)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetContactAngleSensitivity(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetContactAngleSensitivity\n");
	return __PVE_HkCharacterProxyCinfo_GetContactAngleSensitivity(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetContactAngleSensitivity)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetContactAngleSensitivity(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetContactAngleSensitivity\n");
	return __PVE_HkCharacterProxyCinfo_SetContactAngleSensitivity(instance, value);
}

int (*__PVE_HkCharacterProxyCinfo_GetUserPlanes)(void * instance) __attribute__((ms_abi));
int HkCharacterProxyCinfo_GetUserPlanes(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetUserPlanes\n");
	return __PVE_HkCharacterProxyCinfo_GetUserPlanes(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetUserPlanes)(void * instance, int value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetUserPlanes(void * instance, int value) {
	printf("invoke HkCharacterProxyCinfo_SetUserPlanes\n");
	return __PVE_HkCharacterProxyCinfo_SetUserPlanes(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetMaxCharacterSpeedForSolver)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetMaxCharacterSpeedForSolver(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetMaxCharacterSpeedForSolver\n");
	return __PVE_HkCharacterProxyCinfo_GetMaxCharacterSpeedForSolver(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetMaxCharacterSpeedForSolver)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetMaxCharacterSpeedForSolver(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetMaxCharacterSpeedForSolver\n");
	return __PVE_HkCharacterProxyCinfo_SetMaxCharacterSpeedForSolver(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetCharacterStrength)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetCharacterStrength(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetCharacterStrength\n");
	return __PVE_HkCharacterProxyCinfo_GetCharacterStrength(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetCharacterStrength)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetCharacterStrength(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetCharacterStrength\n");
	return __PVE_HkCharacterProxyCinfo_SetCharacterStrength(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetCharacterMass)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetCharacterMass(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetCharacterMass\n");
	return __PVE_HkCharacterProxyCinfo_GetCharacterMass(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetCharacterMass)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetCharacterMass(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetCharacterMass\n");
	return __PVE_HkCharacterProxyCinfo_SetCharacterMass(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetMaxSlope)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetMaxSlope(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetMaxSlope\n");
	return __PVE_HkCharacterProxyCinfo_GetMaxSlope(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetMaxSlope)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetMaxSlope(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetMaxSlope\n");
	return __PVE_HkCharacterProxyCinfo_SetMaxSlope(instance, value);
}

float (*__PVE_HkCharacterProxyCinfo_GetPenetrationRecoverySpeed)(void * instance) __attribute__((ms_abi));
float HkCharacterProxyCinfo_GetPenetrationRecoverySpeed(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetPenetrationRecoverySpeed\n");
	return __PVE_HkCharacterProxyCinfo_GetPenetrationRecoverySpeed(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetPenetrationRecoverySpeed)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetPenetrationRecoverySpeed(void * instance, float value) {
	printf("invoke HkCharacterProxyCinfo_SetPenetrationRecoverySpeed\n");
	return __PVE_HkCharacterProxyCinfo_SetPenetrationRecoverySpeed(instance, value);
}

int (*__PVE_HkCharacterProxyCinfo_GetMaxCastIterations)(void * instance) __attribute__((ms_abi));
int HkCharacterProxyCinfo_GetMaxCastIterations(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetMaxCastIterations\n");
	return __PVE_HkCharacterProxyCinfo_GetMaxCastIterations(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetMaxCastIterations)(void * instance, int value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetMaxCastIterations(void * instance, int value) {
	printf("invoke HkCharacterProxyCinfo_SetMaxCastIterations\n");
	return __PVE_HkCharacterProxyCinfo_SetMaxCastIterations(instance, value);
}

int (*__PVE_HkCharacterProxyCinfo_GetRefreshManifoldInCheckSupport)(void * instance) __attribute__((ms_abi));
int HkCharacterProxyCinfo_GetRefreshManifoldInCheckSupport(void * instance) {
	printf("invoke HkCharacterProxyCinfo_GetRefreshManifoldInCheckSupport\n");
	return __PVE_HkCharacterProxyCinfo_GetRefreshManifoldInCheckSupport(instance);
}

void (*__PVE_HkCharacterProxyCinfo_SetRefreshManifoldInCheckSupport)(void * instance, int value) __attribute__((ms_abi));
void HkCharacterProxyCinfo_SetRefreshManifoldInCheckSupport(void * instance, int value) {
	printf("invoke HkCharacterProxyCinfo_SetRefreshManifoldInCheckSupport\n");
	return __PVE_HkCharacterProxyCinfo_SetRefreshManifoldInCheckSupport(instance, value);
}

void * (*__PVE_HkCharacterRigidBody_Create)(void * characterRigidBodyCinfo, float maxCharacterSpeed) __attribute__((ms_abi));
void * HkCharacterRigidBody_Create(void * characterRigidBodyCinfo, float maxCharacterSpeed) {
	printf("invoke HkCharacterRigidBody_Create\n");
	return __PVE_HkCharacterRigidBody_Create(characterRigidBodyCinfo, maxCharacterSpeed);
}

void * (*__PVE_HkCharacterRigidBody_GetCharacterRigidbody)(void * instance) __attribute__((ms_abi));
void * HkCharacterRigidBody_GetCharacterRigidbody(void * instance) {
	printf("invoke HkCharacterRigidBody_GetCharacterRigidbody\n");
	return __PVE_HkCharacterRigidBody_GetCharacterRigidbody(instance);
}

void (*__PVE_HkCharacterRigidBody_SetWalkingState)(void * instance, void * shape, float jumpHeight, float gainSpeed, float maxCharacterSpeed) __attribute__((ms_abi));
void HkCharacterRigidBody_SetWalkingState(void * instance, void * shape, float jumpHeight, float gainSpeed, float maxCharacterSpeed) {
	printf("invoke HkCharacterRigidBody_SetWalkingState\n");
	return __PVE_HkCharacterRigidBody_SetWalkingState(instance, shape, jumpHeight, gainSpeed, maxCharacterSpeed);
}

void (*__PVE_HkCharacterRigidBody_SetFlyingState)(void * instance, void * shape, float maxCharacterSpeed, float maxAcceleration) __attribute__((ms_abi));
void HkCharacterRigidBody_SetFlyingState(void * instance, void * shape, float maxCharacterSpeed, float maxAcceleration) {
	printf("invoke HkCharacterRigidBody_SetFlyingState\n");
	return __PVE_HkCharacterRigidBody_SetFlyingState(instance, shape, maxCharacterSpeed, maxAcceleration);
}

void (*__PVE_HkCharacterRigidBody_SetLadderState)(void * instance, float maxCharacterSpeed, float maxAcceleration) __attribute__((ms_abi));
void HkCharacterRigidBody_SetLadderState(void * instance, float maxCharacterSpeed, float maxAcceleration) {
	printf("invoke HkCharacterRigidBody_SetLadderState\n");
	return __PVE_HkCharacterRigidBody_SetLadderState(instance, maxCharacterSpeed, maxAcceleration);
}

void (*__PVE_HkCharacterRigidBody_SetDefaultShape)(void * instance, void * shape) __attribute__((ms_abi));
void HkCharacterRigidBody_SetDefaultShape(void * instance, void * shape) {
	printf("invoke HkCharacterRigidBody_SetDefaultShape\n");
	return __PVE_HkCharacterRigidBody_SetDefaultShape(instance, shape);
}

void (*__PVE_HkCharacterRigidBody_SetShapeForCrouch)(void * instance, void * shape) __attribute__((ms_abi));
void HkCharacterRigidBody_SetShapeForCrouch(void * instance, void * shape) {
	printf("invoke HkCharacterRigidBody_SetShapeForCrouch\n");
	return __PVE_HkCharacterRigidBody_SetShapeForCrouch(instance, shape);
}

struct Vector3 (*__PVE_HkCharacterRigidBody_GetPosition)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterRigidBody_GetPosition(void * instance) {
	printf("invoke HkCharacterRigidBody_GetPosition\n");
	return __PVE_HkCharacterRigidBody_GetPosition(instance);
}

void (*__PVE_HkCharacterRigidBody_SetPosition)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterRigidBody_SetPosition(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterRigidBody_SetPosition\n");
	return __PVE_HkCharacterRigidBody_SetPosition(instance, value);
}

int (*__PVE_HkCharacterRigidBody_GetState)(void * instance) __attribute__((ms_abi));
int HkCharacterRigidBody_GetState(void * instance) {
	printf("invoke HkCharacterRigidBody_GetState\n");
	return __PVE_HkCharacterRigidBody_GetState(instance);
}

void (*__PVE_HkCharacterRigidBody_SetState)(void * instance, int value) __attribute__((ms_abi));
void HkCharacterRigidBody_SetState(void * instance, int value) {
	printf("invoke HkCharacterRigidBody_SetState\n");
	return __PVE_HkCharacterRigidBody_SetState(instance, value);
}

void (*__PVE_HkCharacterRigidBody_StepSimulation)(void * instance, float timeInSec, int Jump, int WantJump, int AtLadder, float PosX, float PosY, float Speed, float Elevate, struct Vector3 Up, struct Vector3 Forward, struct Vector3 ElevateVector, struct Vector3 ElevateUpVector, struct Vector3 Gravity, float myJumpHeight, void * AngularVelocity) __attribute__((ms_abi));
void HkCharacterRigidBody_StepSimulation(void * instance, float timeInSec, int Jump, int WantJump, int AtLadder, float PosX, float PosY, float Speed, float Elevate, struct Vector3 Up, struct Vector3 Forward, struct Vector3 ElevateVector, struct Vector3 ElevateUpVector, struct Vector3 Gravity, float myJumpHeight, void * AngularVelocity) {
	printf("invoke HkCharacterRigidBody_StepSimulation\n");
	return __PVE_HkCharacterRigidBody_StepSimulation(instance, timeInSec, Jump, WantJump, AtLadder, PosX, PosY, Speed, Elevate, Up, Forward, ElevateVector, ElevateUpVector, Gravity, myJumpHeight, AngularVelocity);
}

void (*__PVE_HkCharacterRigidBody_UpdateVelocity)(void * instance, float timeInSec, int Supported, struct Vector3 AngularVelocity, struct Quaternion DesiredOrientation) __attribute__((ms_abi));
void HkCharacterRigidBody_UpdateVelocity(void * instance, float timeInSec, int Supported, struct Vector3 AngularVelocity, struct Quaternion DesiredOrientation) {
	printf("invoke HkCharacterRigidBody_UpdateVelocity\n");
	return __PVE_HkCharacterRigidBody_UpdateVelocity(instance, timeInSec, Supported, AngularVelocity, DesiredOrientation);
}

void (*__PVE_HkCharacterRigidBody_UpdateSupport)(void * instance, float timeInSec) __attribute__((ms_abi));
void HkCharacterRigidBody_UpdateSupport(void * instance, float timeInSec) {
	printf("invoke HkCharacterRigidBody_UpdateSupport\n");
	return __PVE_HkCharacterRigidBody_UpdateSupport(instance, timeInSec);
}

void (*__PVE_HkCharacterRigidBody_SetRigidBodyTransform)(void * instance, struct Matrix world) __attribute__((ms_abi));
void HkCharacterRigidBody_SetRigidBodyTransform(void * instance, struct Matrix world) {
	printf("invoke HkCharacterRigidBody_SetRigidBodyTransform\n");
	return __PVE_HkCharacterRigidBody_SetRigidBodyTransform(instance, world);
}

struct Matrix (*__PVE_HkCharacterRigidBody_GetRigidBodyTransform)(void * instance) __attribute__((ms_abi));
struct Matrix HkCharacterRigidBody_GetRigidBodyTransform(void * instance) {
	printf("invoke HkCharacterRigidBody_GetRigidBodyTransform\n");
	return __PVE_HkCharacterRigidBody_GetRigidBodyTransform(instance);
}

struct Vector3 (*__PVE_HkCharacterRigidBody_GetLinearVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterRigidBody_GetLinearVelocity(void * instance) {
	printf("invoke HkCharacterRigidBody_GetLinearVelocity\n");
	return __PVE_HkCharacterRigidBody_GetLinearVelocity(instance);
}

void (*__PVE_HkCharacterRigidBody_SetLinearVelocity)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterRigidBody_SetLinearVelocity(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterRigidBody_SetLinearVelocity\n");
	return __PVE_HkCharacterRigidBody_SetLinearVelocity(instance, value);
}

void (*__PVE_HkCharacterRigidBody_ApplyLinearImpulse)(void * instance, struct Vector3 impulse) __attribute__((ms_abi));
void HkCharacterRigidBody_ApplyLinearImpulse(void * instance, struct Vector3 impulse) {
	printf("invoke HkCharacterRigidBody_ApplyLinearImpulse\n");
	return __PVE_HkCharacterRigidBody_ApplyLinearImpulse(instance, impulse);
}

void (*__PVE_HkCharacterRigidBody_ApplyAngularImpulse)(void * instance, struct Vector3 impulse) __attribute__((ms_abi));
void HkCharacterRigidBody_ApplyAngularImpulse(void * instance, struct Vector3 impulse) {
	printf("invoke HkCharacterRigidBody_ApplyAngularImpulse\n");
	return __PVE_HkCharacterRigidBody_ApplyAngularImpulse(instance, impulse);
}

void (*__PVE_HkCharacterRigidBody_SetSupportDistance)(void * instance, float distance) __attribute__((ms_abi));
void HkCharacterRigidBody_SetSupportDistance(void * instance, float distance) {
	printf("invoke HkCharacterRigidBody_SetSupportDistance\n");
	return __PVE_HkCharacterRigidBody_SetSupportDistance(instance, distance);
}

void (*__PVE_HkCharacterRigidBody_SetHardSupportDistance)(void * instance, float distance) __attribute__((ms_abi));
void HkCharacterRigidBody_SetHardSupportDistance(void * instance, float distance) {
	printf("invoke HkCharacterRigidBody_SetHardSupportDistance\n");
	return __PVE_HkCharacterRigidBody_SetHardSupportDistance(instance, distance);
}

struct Vector3 (*__PVE_HkCharacterRigidBody_GetAngularVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterRigidBody_GetAngularVelocity(void * instance) {
	printf("invoke HkCharacterRigidBody_GetAngularVelocity\n");
	return __PVE_HkCharacterRigidBody_GetAngularVelocity(instance);
}

void (*__PVE_HkCharacterRigidBody_SetAngularVelocity)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterRigidBody_SetAngularVelocity(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterRigidBody_SetAngularVelocity\n");
	return __PVE_HkCharacterRigidBody_SetAngularVelocity(instance, value);
}

int (*__PVE_HkCharacterRigidBody_IsSupportedByFloatingObject)(void * instance) __attribute__((ms_abi));
int HkCharacterRigidBody_IsSupportedByFloatingObject(void * instance) {
	printf("invoke HkCharacterRigidBody_IsSupportedByFloatingObject\n");
	return __PVE_HkCharacterRigidBody_IsSupportedByFloatingObject(instance);
}

int (*__PVE_HkCharacterRigidBody_IsSupported)(void * instance) __attribute__((ms_abi));
int HkCharacterRigidBody_IsSupported(void * instance) {
	printf("invoke HkCharacterRigidBody_IsSupported\n");
	return __PVE_HkCharacterRigidBody_IsSupported(instance);
}

struct Vector3 (*__PVE_HkCharacterRigidBody_GetSupportNormal)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterRigidBody_GetSupportNormal(void * instance) {
	printf("invoke HkCharacterRigidBody_GetSupportNormal\n");
	return __PVE_HkCharacterRigidBody_GetSupportNormal(instance);
}

struct Vector3 (*__PVE_HkCharacterRigidBody_GetGroundVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterRigidBody_GetGroundVelocity(void * instance) {
	printf("invoke HkCharacterRigidBody_GetGroundVelocity\n");
	return __PVE_HkCharacterRigidBody_GetGroundVelocity(instance);
}

int (*__PVE_HkCharacterRigidBody_GetUseSupportInfoQuery)(void * instance) __attribute__((ms_abi));
int HkCharacterRigidBody_GetUseSupportInfoQuery(void * instance) {
	printf("invoke HkCharacterRigidBody_GetUseSupportInfoQuery\n");
	return __PVE_HkCharacterRigidBody_GetUseSupportInfoQuery(instance);
}

void (*__PVE_HkCharacterRigidBody_SetUseSupportInfoQuery)(void * instance, int value) __attribute__((ms_abi));
void HkCharacterRigidBody_SetUseSupportInfoQuery(void * instance, int value) {
	printf("invoke HkCharacterRigidBody_SetUseSupportInfoQuery\n");
	return __PVE_HkCharacterRigidBody_SetUseSupportInfoQuery(instance, value);
}

void (*__PVE_HkCharacterRigidBody_SetPreviousSupportedState)(void * instance, int supported) __attribute__((ms_abi));
void HkCharacterRigidBody_SetPreviousSupportedState(void * instance, int supported) {
	printf("invoke HkCharacterRigidBody_SetPreviousSupportedState\n");
	return __PVE_HkCharacterRigidBody_SetPreviousSupportedState(instance, supported);
}

void (*__PVE_HkCharacterRigidBody_ResetSurfaceVelocity)(void * instance) __attribute__((ms_abi));
void HkCharacterRigidBody_ResetSurfaceVelocity(void * instance) {
	printf("invoke HkCharacterRigidBody_ResetSurfaceVelocity\n");
	return __PVE_HkCharacterRigidBody_ResetSurfaceVelocity(instance);
}

void (*__PVE_HkCharacterRigidBody_SetMaxSlope)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBody_SetMaxSlope(void * instance, float value) {
	printf("invoke HkCharacterRigidBody_SetMaxSlope\n");
	return __PVE_HkCharacterRigidBody_SetMaxSlope(instance, value);
}

float (*__PVE_HkCharacterRigidBody_GetMaxSlope)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBody_GetMaxSlope(void * instance) {
	printf("invoke HkCharacterRigidBody_GetMaxSlope\n");
	return __PVE_HkCharacterRigidBody_GetMaxSlope(instance);
}

void (*__PVE_HkCharacterRigidBody_GetSupportBodies)(void * instance, void * size, void * version, void * list) __attribute__((ms_abi));
void HkCharacterRigidBody_GetSupportBodies(void * instance, void * size, void * version, void * list) {
	printf("invoke HkCharacterRigidBody_GetSupportBodies\n");
	return __PVE_HkCharacterRigidBody_GetSupportBodies(instance, size, version, list);
}

void * (*__PVE_HkCharacterRigidBodyCinfo_Create)() __attribute__((ms_abi));
void * HkCharacterRigidBodyCinfo_Create() {
	printf("invoke HkCharacterRigidBodyCinfo_Create\n");
	return __PVE_HkCharacterRigidBodyCinfo_Create();
}

int (*__PVE_HkCharacterRigidBodyCinfo_GetCollisionFilterInfo)(void * instance) __attribute__((ms_abi));
int HkCharacterRigidBodyCinfo_GetCollisionFilterInfo(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetCollisionFilterInfo\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetCollisionFilterInfo(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetCollisionFilterInfo)(void * instance, int value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetCollisionFilterInfo(void * instance, int value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetCollisionFilterInfo\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetCollisionFilterInfo(instance, value);
}

void * (*__PVE_HkCharacterRigidBodyCinfo_GetShape)(void * instance) __attribute__((ms_abi));
void * HkCharacterRigidBodyCinfo_GetShape(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetShape\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetShape(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetShape)(void * instance, void * value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetShape(void * instance, void * value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetShape\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetShape(instance, value);
}

struct Vector3 (*__PVE_HkCharacterRigidBodyCinfo_GetPosition)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterRigidBodyCinfo_GetPosition(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetPosition\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetPosition(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetPosition)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetPosition(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetPosition\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetPosition(instance, value);
}

struct Quaternion (*__PVE_HkCharacterRigidBodyCinfo_GetRotation)(void * instance) __attribute__((ms_abi));
struct Quaternion HkCharacterRigidBodyCinfo_GetRotation(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetRotation\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetRotation(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetRotation)(void * instance, struct Quaternion value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetRotation(void * instance, struct Quaternion value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetRotation\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetRotation(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetMass)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetMass(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetMass\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetMass(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetMass)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetMass(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetMass\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetMass(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetFriction)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetFriction(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetFriction\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetFriction(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetFriction)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetFriction(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetFriction\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetFriction(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetMaxLinearVelocity)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetMaxLinearVelocity(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetMaxLinearVelocity\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetMaxLinearVelocity(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetMaxLinearVelocity)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetMaxLinearVelocity(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetMaxLinearVelocity\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetMaxLinearVelocity(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetAllowedPenetrationDepth)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetAllowedPenetrationDepth(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetAllowedPenetrationDepth\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetAllowedPenetrationDepth(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetAllowedPenetrationDepth)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetAllowedPenetrationDepth(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetAllowedPenetrationDepth\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetAllowedPenetrationDepth(instance, value);
}

struct Vector3 (*__PVE_HkCharacterRigidBodyCinfo_GetUp)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCharacterRigidBodyCinfo_GetUp(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetUp\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetUp(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetUp)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetUp(void * instance, struct Vector3 value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetUp\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetUp(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetMaxSlope)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetMaxSlope(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetMaxSlope\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetMaxSlope(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetMaxSlope)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetMaxSlope(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetMaxSlope\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetMaxSlope(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetMaxForce)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetMaxForce(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetMaxForce\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetMaxForce(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetMaxForce)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetMaxForce(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetMaxForce\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetMaxForce(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetUnweldingHeightOffsetFactor)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetUnweldingHeightOffsetFactor(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetUnweldingHeightOffsetFactor\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetUnweldingHeightOffsetFactor(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetUnweldingHeightOffsetFactor)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetUnweldingHeightOffsetFactor(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetUnweldingHeightOffsetFactor\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetUnweldingHeightOffsetFactor(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetMaxSpeedForSimplexSolver)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetMaxSpeedForSimplexSolver(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetMaxSpeedForSimplexSolver\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetMaxSpeedForSimplexSolver(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetMaxSpeedForSimplexSolver)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetMaxSpeedForSimplexSolver(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetMaxSpeedForSimplexSolver\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetMaxSpeedForSimplexSolver(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetSupportDistance)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetSupportDistance(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetSupportDistance\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetSupportDistance(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetSupportDistance)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetSupportDistance(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetSupportDistance\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetSupportDistance(instance, value);
}

float (*__PVE_HkCharacterRigidBodyCinfo_GetHardSupportDistance)(void * instance) __attribute__((ms_abi));
float HkCharacterRigidBodyCinfo_GetHardSupportDistance(void * instance) {
	printf("invoke HkCharacterRigidBodyCinfo_GetHardSupportDistance\n");
	return __PVE_HkCharacterRigidBodyCinfo_GetHardSupportDistance(instance);
}

void (*__PVE_HkCharacterRigidBodyCinfo_SetHardSupportDistance)(void * instance, float value) __attribute__((ms_abi));
void HkCharacterRigidBodyCinfo_SetHardSupportDistance(void * instance, float value) {
	printf("invoke HkCharacterRigidBodyCinfo_SetHardSupportDistance\n");
	return __PVE_HkCharacterRigidBodyCinfo_SetHardSupportDistance(instance, value);
}

void * (*__PVE_HkBallAndSocketConstraintData_Create)() __attribute__((ms_abi));
void * HkBallAndSocketConstraintData_Create() {
	printf("invoke HkBallAndSocketConstraintData_Create\n");
	return __PVE_HkBallAndSocketConstraintData_Create();
}

void (*__PVE_HkBallAndSocketConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 pivotA, struct Vector3 pivotB) __attribute__((ms_abi));
void HkBallAndSocketConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 pivotA, struct Vector3 pivotB) {
	printf("invoke HkBallAndSocketConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkBallAndSocketConstraintData_SetInBodySpaceInternal(instance, pivotA, pivotB);
}

void * (*__PVE_HkBreakableConstraintData_Create)(void * data) __attribute__((ms_abi));
void * HkBreakableConstraintData_Create(void * data) {
	printf("invoke HkBreakableConstraintData_Create\n");
	return __PVE_HkBreakableConstraintData_Create(data);
}

float (*__PVE_HkBreakableConstraintData_GetThreshold)(void * instance) __attribute__((ms_abi));
float HkBreakableConstraintData_GetThreshold(void * instance) {
	printf("invoke HkBreakableConstraintData_GetThreshold\n");
	return __PVE_HkBreakableConstraintData_GetThreshold(instance);
}

void (*__PVE_HkBreakableConstraintData_SetThreshold)(void * instance, float value) __attribute__((ms_abi));
void HkBreakableConstraintData_SetThreshold(void * instance, float value) {
	printf("invoke HkBreakableConstraintData_SetThreshold\n");
	return __PVE_HkBreakableConstraintData_SetThreshold(instance, value);
}

int (*__PVE_HkBreakableConstraintData_GetRemoveFromWorldOnBrake)(void * instance) __attribute__((ms_abi));
int HkBreakableConstraintData_GetRemoveFromWorldOnBrake(void * instance) {
	printf("invoke HkBreakableConstraintData_GetRemoveFromWorldOnBrake\n");
	return __PVE_HkBreakableConstraintData_GetRemoveFromWorldOnBrake(instance);
}

void (*__PVE_HkBreakableConstraintData_SetRemoveFromWorldOnBrake)(void * instance, int value) __attribute__((ms_abi));
void HkBreakableConstraintData_SetRemoveFromWorldOnBrake(void * instance, int value) {
	printf("invoke HkBreakableConstraintData_SetRemoveFromWorldOnBrake\n");
	return __PVE_HkBreakableConstraintData_SetRemoveFromWorldOnBrake(instance, value);
}

int (*__PVE_HkBreakableConstraintData_GetReapplyVelocityOnBreak)(void * instance) __attribute__((ms_abi));
int HkBreakableConstraintData_GetReapplyVelocityOnBreak(void * instance) {
	printf("invoke HkBreakableConstraintData_GetReapplyVelocityOnBreak\n");
	return __PVE_HkBreakableConstraintData_GetReapplyVelocityOnBreak(instance);
}

void (*__PVE_HkBreakableConstraintData_SetReapplyVelocityOnBreak)(void * instance, int value) __attribute__((ms_abi));
void HkBreakableConstraintData_SetReapplyVelocityOnBreak(void * instance, int value) {
	printf("invoke HkBreakableConstraintData_SetReapplyVelocityOnBreak\n");
	return __PVE_HkBreakableConstraintData_SetReapplyVelocityOnBreak(instance, value);
}

int (*__PVE_HkBreakableConstraintData_GetIsBroken)(void * instance, void * constraint) __attribute__((ms_abi));
int HkBreakableConstraintData_GetIsBroken(void * instance, void * constraint) {
	printf("invoke HkBreakableConstraintData_GetIsBroken\n");
	return __PVE_HkBreakableConstraintData_GetIsBroken(instance, constraint);
}

void * (*__PVE_HkCogWheelConstraintData_Create)() __attribute__((ms_abi));
void * HkCogWheelConstraintData_Create() {
	printf("invoke HkCogWheelConstraintData_Create\n");
	return __PVE_HkCogWheelConstraintData_Create();
}

void (*__PVE_HkCogWheelConstraintData_SetInWorldSpace)(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Vector3 rotationPivotA, struct Vector3 rotationAxisA, float radiusA, struct Vector3 rotationPivotB, struct Vector3 rotationAxisB, float radiusB) __attribute__((ms_abi));
void HkCogWheelConstraintData_SetInWorldSpace(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Vector3 rotationPivotA, struct Vector3 rotationAxisA, float radiusA, struct Vector3 rotationPivotB, struct Vector3 rotationAxisB, float radiusB) {
	printf("invoke HkCogWheelConstraintData_SetInWorldSpace\n");
	return __PVE_HkCogWheelConstraintData_SetInWorldSpace(instance, bodyATransform, bodyBTransform, rotationPivotA, rotationAxisA, radiusA, rotationPivotB, rotationAxisB, radiusB);
}

void (*__PVE_HkCogWheelConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 rotationPivotAInA, struct Vector3 rotationAxisAInA, float radiusA, struct Vector3 rotationPivotBInB, struct Vector3 rotationAxisBInB, float radiusB) __attribute__((ms_abi));
void HkCogWheelConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 rotationPivotAInA, struct Vector3 rotationAxisAInA, float radiusA, struct Vector3 rotationPivotBInB, struct Vector3 rotationAxisBInB, float radiusB) {
	printf("invoke HkCogWheelConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkCogWheelConstraintData_SetInBodySpaceInternal(instance, rotationPivotAInA, rotationAxisAInA, radiusA, rotationPivotBInB, rotationAxisBInB, radiusB);
}

void * (*__PVE_HkConstraint_Create)(void * entityA, void * entityB, void * data, int priority) __attribute__((ms_abi));
void * HkConstraint_Create(void * entityA, void * entityB, void * data, int priority) {
	printf("invoke HkConstraint_Create\n");
	return __PVE_HkConstraint_Create(entityA, entityB, data, priority);
}

void (*__PVE_HkConstraint_AddConstraintListener)(void * instance, void * listener) __attribute__((ms_abi));
void HkConstraint_AddConstraintListener(void * instance, void * listener) {
	printf("invoke HkConstraint_AddConstraintListener\n");
	return __PVE_HkConstraint_AddConstraintListener(instance, listener);
}

void (*__PVE_HkConstraint_RemoveConstraintListener)(void * instance, void * listener) __attribute__((ms_abi));
void HkConstraint_RemoveConstraintListener(void * instance, void * listener) {
	printf("invoke HkConstraint_RemoveConstraintListener\n");
	return __PVE_HkConstraint_RemoveConstraintListener(instance, listener);
}

void (*__PVE_HkConstraint_ReplaceEntity)(void * instance, void * oldEntity, void * newEntity) __attribute__((ms_abi));
void HkConstraint_ReplaceEntity(void * instance, void * oldEntity, void * newEntity) {
	printf("invoke HkConstraint_ReplaceEntity\n");
	return __PVE_HkConstraint_ReplaceEntity(instance, oldEntity, newEntity);
}

void (*__PVE_HkConstraint_SetVirtualMassInverse)(void * instance, struct Vector4 invMassA, struct Vector4 invMassB) __attribute__((ms_abi));
void HkConstraint_SetVirtualMassInverse(void * instance, struct Vector4 invMassA, struct Vector4 invMassB) {
	printf("invoke HkConstraint_SetVirtualMassInverse\n");
	return __PVE_HkConstraint_SetVirtualMassInverse(instance, invMassA, invMassB);
}

int (*__PVE_HkConstraint_GetPriority)(void * instance) __attribute__((ms_abi));
int HkConstraint_GetPriority(void * instance) {
	printf("invoke HkConstraint_GetPriority\n");
	return __PVE_HkConstraint_GetPriority(instance);
}

void (*__PVE_HkConstraint_SetPriority)(void * instance, int value) __attribute__((ms_abi));
void HkConstraint_SetPriority(void * instance, int value) {
	printf("invoke HkConstraint_SetPriority\n");
	return __PVE_HkConstraint_SetPriority(instance, value);
}

int (*__PVE_HkConstraint_GetWantRuntime)(void * instance) __attribute__((ms_abi));
int HkConstraint_GetWantRuntime(void * instance) {
	printf("invoke HkConstraint_GetWantRuntime\n");
	return __PVE_HkConstraint_GetWantRuntime(instance);
}

void (*__PVE_HkConstraint_SetWantRuntime)(void * instance, int value) __attribute__((ms_abi));
void HkConstraint_SetWantRuntime(void * instance, int value) {
	printf("invoke HkConstraint_SetWantRuntime\n");
	return __PVE_HkConstraint_SetWantRuntime(instance, value);
}

int (*__PVE_HkConstraint_IsInWorld)(void * instance) __attribute__((ms_abi));
int HkConstraint_IsInWorld(void * instance) {
	printf("invoke HkConstraint_IsInWorld\n");
	return __PVE_HkConstraint_IsInWorld(instance);
}

void * (*__PVE_HkConstraint_GetRigidBodyA)(void * instance) __attribute__((ms_abi));
void * HkConstraint_GetRigidBodyA(void * instance) {
	printf("invoke HkConstraint_GetRigidBodyA\n");
	return __PVE_HkConstraint_GetRigidBodyA(instance);
}

void * (*__PVE_HkConstraint_GetRigidBodyB)(void * instance) __attribute__((ms_abi));
void * HkConstraint_GetRigidBodyB(void * instance) {
	printf("invoke HkConstraint_GetRigidBodyB\n");
	return __PVE_HkConstraint_GetRigidBodyB(instance);
}

int (*__PVE_HkConstraint_GetEnabled)(void * instance) __attribute__((ms_abi));
int HkConstraint_GetEnabled(void * instance) {
	printf("invoke HkConstraint_GetEnabled\n");
	return __PVE_HkConstraint_GetEnabled(instance);
}

void (*__PVE_HkConstraint_SetEnabled)(void * instance, int value) __attribute__((ms_abi));
void HkConstraint_SetEnabled(void * instance, int value) {
	printf("invoke HkConstraint_SetEnabled\n");
	return __PVE_HkConstraint_SetEnabled(instance, value);
}

void (*__PVE_HkConstraint_GetPivotsInWorld)(void * instance, void * outPivotA, void * outPivotB) __attribute__((ms_abi));
void HkConstraint_GetPivotsInWorld(void * instance, void * outPivotA, void * outPivotB) {
	printf("invoke HkConstraint_GetPivotsInWorld\n");
	return __PVE_HkConstraint_GetPivotsInWorld(instance, outPivotA, outPivotB);
}

long int (*__PVE_HkConstraint_GetUserData)(void * instance) __attribute__((ms_abi));
long int HkConstraint_GetUserData(void * instance) {
	printf("invoke HkConstraint_GetUserData\n");
	return __PVE_HkConstraint_GetUserData(instance);
}

void (*__PVE_HkConstraint_SetUserData)(void * instance, long int value) __attribute__((ms_abi));
void HkConstraint_SetUserData(void * instance, long int value) {
	printf("invoke HkConstraint_SetUserData\n");
	return __PVE_HkConstraint_SetUserData(instance, value);
}

void (*__PVE_HkConstraint_AddCenterOfMassModifierAtom)(void * instance, struct Vector3 modifierA, struct Vector3 modifierB) __attribute__((ms_abi));
void HkConstraint_AddCenterOfMassModifierAtom(void * instance, struct Vector3 modifierA, struct Vector3 modifierB) {
	printf("invoke HkConstraint_AddCenterOfMassModifierAtom\n");
	return __PVE_HkConstraint_AddCenterOfMassModifierAtom(instance, modifierA, modifierB);
}

void (*__PVE_HkConstraint_FindConnectedConstraints)(void * rigidBody, void * reader, void * userData) __attribute__((ms_abi));
void HkConstraint_FindConnectedConstraints(void * rigidBody, void * reader, void * userData) {
	printf("invoke HkConstraint_FindConnectedConstraints\n");
	return __PVE_HkConstraint_FindConnectedConstraints(rigidBody, _PVE_Trampoline_Havok_HkConstraint_ReadConstraintsCallback(reader), userData);
}

float (*__PVE_HkConstraintData_GetMaximumLinearImpulse)(void * instance) __attribute__((ms_abi));
float HkConstraintData_GetMaximumLinearImpulse(void * instance) {
	printf("invoke HkConstraintData_GetMaximumLinearImpulse\n");
	return __PVE_HkConstraintData_GetMaximumLinearImpulse(instance);
}

void (*__PVE_HkConstraintData_SetMaximumLinearImpulse)(void * instance, float value) __attribute__((ms_abi));
void HkConstraintData_SetMaximumLinearImpulse(void * instance, float value) {
	printf("invoke HkConstraintData_SetMaximumLinearImpulse\n");
	return __PVE_HkConstraintData_SetMaximumLinearImpulse(instance, value);
}

float (*__PVE_HkConstraintData_GetMaximumAngularImpulse)(void * instance) __attribute__((ms_abi));
float HkConstraintData_GetMaximumAngularImpulse(void * instance) {
	printf("invoke HkConstraintData_GetMaximumAngularImpulse\n");
	return __PVE_HkConstraintData_GetMaximumAngularImpulse(instance);
}

void (*__PVE_HkConstraintData_SetMaximumAngularImpulse)(void * instance, float value) __attribute__((ms_abi));
void HkConstraintData_SetMaximumAngularImpulse(void * instance, float value) {
	printf("invoke HkConstraintData_SetMaximumAngularImpulse\n");
	return __PVE_HkConstraintData_SetMaximumAngularImpulse(instance, value);
}

float (*__PVE_HkConstraintData_GetBreachImpulse)(void * instance) __attribute__((ms_abi));
float HkConstraintData_GetBreachImpulse(void * instance) {
	printf("invoke HkConstraintData_GetBreachImpulse\n");
	return __PVE_HkConstraintData_GetBreachImpulse(instance);
}

void (*__PVE_HkConstraintData_SetBreachImpulse)(void * instance, float value) __attribute__((ms_abi));
void HkConstraintData_SetBreachImpulse(void * instance, float value) {
	printf("invoke HkConstraintData_SetBreachImpulse\n");
	return __PVE_HkConstraintData_SetBreachImpulse(instance, value);
}

float (*__PVE_HkConstraintData_GetInertiaStabilizationFactor)(void * instance) __attribute__((ms_abi));
float HkConstraintData_GetInertiaStabilizationFactor(void * instance) {
	printf("invoke HkConstraintData_GetInertiaStabilizationFactor\n");
	return __PVE_HkConstraintData_GetInertiaStabilizationFactor(instance);
}

void (*__PVE_HkConstraintData_SetInertiaStabilizationFactor)(void * instance, float value) __attribute__((ms_abi));
void HkConstraintData_SetInertiaStabilizationFactor(void * instance, float value) {
	printf("invoke HkConstraintData_SetInertiaStabilizationFactor\n");
	return __PVE_HkConstraintData_SetInertiaStabilizationFactor(instance, value);
}

void (*__PVE_HkConstraintData_SetSolvingMethod)(void * instance, int method) __attribute__((ms_abi));
void HkConstraintData_SetSolvingMethod(void * instance, int method) {
	printf("invoke HkConstraintData_SetSolvingMethod\n");
	return __PVE_HkConstraintData_SetSolvingMethod(instance, method);
}

void * (*__PVE_HkConstraintListener_Create)() __attribute__((ms_abi));
void * HkConstraintListener_Create() {
	printf("invoke HkConstraintListener_Create\n");
	return __PVE_HkConstraintListener_Create();
}

void (*__PVE_HkConstraintListener_Release)(void * instance) __attribute__((ms_abi));
void HkConstraintListener_Release(void * instance) {
	printf("invoke HkConstraintListener_Release\n");
	return __PVE_HkConstraintListener_Release(instance);
}

void (*__PVE_HkConstraintListener_SetCallbacks)(void * instance, void * onAdded, void * onRemoved, void * onBreaking) __attribute__((ms_abi));
void HkConstraintListener_SetCallbacks(void * instance, void * onAdded, void * onRemoved, void * onBreaking) {
	printf("invoke HkConstraintListener_SetCallbacks\n");
	return __PVE_HkConstraintListener_SetCallbacks(instance, _PVE_Trampoline_Havok_HkConstraintListener_OnAdded(onAdded), _PVE_Trampoline_Havok_HkConstraintListener_OnRemoved(onRemoved), _PVE_Trampoline_Havok_HkConstraintListener_OnBreaking(onBreaking));
}

void * (*__PVE_HkCustomWheelConstraintData_Create)() __attribute__((ms_abi));
void * HkCustomWheelConstraintData_Create() {
	printf("invoke HkCustomWheelConstraintData_Create\n");
	return __PVE_HkCustomWheelConstraintData_Create();
}

int (*__PVE_HkCustomWheelConstraintData_GetLimitsEnabled)(void * instance) __attribute__((ms_abi));
int HkCustomWheelConstraintData_GetLimitsEnabled(void * instance) {
	printf("invoke HkCustomWheelConstraintData_GetLimitsEnabled\n");
	return __PVE_HkCustomWheelConstraintData_GetLimitsEnabled(instance);
}

void (*__PVE_HkCustomWheelConstraintData_SetLimitsEnabled)(void * instance, int value) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetLimitsEnabled(void * instance, int value) {
	printf("invoke HkCustomWheelConstraintData_SetLimitsEnabled\n");
	return __PVE_HkCustomWheelConstraintData_SetLimitsEnabled(instance, value);
}

float (*__PVE_HkCustomWheelConstraintData_GetSuspensionMinLimit)(void * instance) __attribute__((ms_abi));
float HkCustomWheelConstraintData_GetSuspensionMinLimit(void * instance) {
	printf("invoke HkCustomWheelConstraintData_GetSuspensionMinLimit\n");
	return __PVE_HkCustomWheelConstraintData_GetSuspensionMinLimit(instance);
}

void (*__PVE_HkCustomWheelConstraintData_SetSuspensionMinLimit)(void * instance, float value) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetSuspensionMinLimit(void * instance, float value) {
	printf("invoke HkCustomWheelConstraintData_SetSuspensionMinLimit\n");
	return __PVE_HkCustomWheelConstraintData_SetSuspensionMinLimit(instance, value);
}

float (*__PVE_HkCustomWheelConstraintData_GetSuspensionMaxLimit)(void * instance) __attribute__((ms_abi));
float HkCustomWheelConstraintData_GetSuspensionMaxLimit(void * instance) {
	printf("invoke HkCustomWheelConstraintData_GetSuspensionMaxLimit\n");
	return __PVE_HkCustomWheelConstraintData_GetSuspensionMaxLimit(instance);
}

void (*__PVE_HkCustomWheelConstraintData_SetSuspensionMaxLimit)(void * instance, float value) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetSuspensionMaxLimit(void * instance, float value) {
	printf("invoke HkCustomWheelConstraintData_SetSuspensionMaxLimit\n");
	return __PVE_HkCustomWheelConstraintData_SetSuspensionMaxLimit(instance, value);
}

int (*__PVE_HkCustomWheelConstraintData_GetFrictionEnabled)(void * instance) __attribute__((ms_abi));
int HkCustomWheelConstraintData_GetFrictionEnabled(void * instance) {
	printf("invoke HkCustomWheelConstraintData_GetFrictionEnabled\n");
	return __PVE_HkCustomWheelConstraintData_GetFrictionEnabled(instance);
}

void (*__PVE_HkCustomWheelConstraintData_SetFrictionEnabled)(void * instance, int value) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetFrictionEnabled(void * instance, int value) {
	printf("invoke HkCustomWheelConstraintData_SetFrictionEnabled\n");
	return __PVE_HkCustomWheelConstraintData_SetFrictionEnabled(instance, value);
}

float (*__PVE_HkCustomWheelConstraintData_GetMaxFrictionTorque)(void * instance) __attribute__((ms_abi));
float HkCustomWheelConstraintData_GetMaxFrictionTorque(void * instance) {
	printf("invoke HkCustomWheelConstraintData_GetMaxFrictionTorque\n");
	return __PVE_HkCustomWheelConstraintData_GetMaxFrictionTorque(instance);
}

void (*__PVE_HkCustomWheelConstraintData_SetMaxFrictionTorque)(void * instance, float value) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetMaxFrictionTorque(void * instance, float value) {
	printf("invoke HkCustomWheelConstraintData_SetMaxFrictionTorque\n");
	return __PVE_HkCustomWheelConstraintData_SetMaxFrictionTorque(instance, value);
}

void (*__PVE_HkCustomWheelConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 axleA, struct Vector3 axleB, struct Vector3 suspensionAxisB, struct Vector3 steeringAxisB) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 axleA, struct Vector3 axleB, struct Vector3 suspensionAxisB, struct Vector3 steeringAxisB) {
	printf("invoke HkCustomWheelConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkCustomWheelConstraintData_SetInBodySpaceInternal(instance, pivotA, pivotB, axleA, axleB, suspensionAxisB, steeringAxisB);
}

void (*__PVE_HkCustomWheelConstraintData_SetSuspensionStrength)(void * instance, float value) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetSuspensionStrength(void * instance, float value) {
	printf("invoke HkCustomWheelConstraintData_SetSuspensionStrength\n");
	return __PVE_HkCustomWheelConstraintData_SetSuspensionStrength(instance, value);
}

void (*__PVE_HkCustomWheelConstraintData_SetSuspensionDamping)(void * instance, float value) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetSuspensionDamping(void * instance, float value) {
	printf("invoke HkCustomWheelConstraintData_SetSuspensionDamping\n");
	return __PVE_HkCustomWheelConstraintData_SetSuspensionDamping(instance, value);
}

void (*__PVE_HkCustomWheelConstraintData_SetSteeringAngle)(void * instance, float value) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetSteeringAngle(void * instance, float value) {
	printf("invoke HkCustomWheelConstraintData_SetSteeringAngle\n");
	return __PVE_HkCustomWheelConstraintData_SetSteeringAngle(instance, value);
}

void (*__PVE_HkCustomWheelConstraintData_SetAngleLimits)(void * instance, float min, float max) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetAngleLimits(void * instance, float min, float max) {
	printf("invoke HkCustomWheelConstraintData_SetAngleLimits\n");
	return __PVE_HkCustomWheelConstraintData_SetAngleLimits(instance, min, max);
}

float (*__PVE_HkCustomWheelConstraintData_GetAngleLimitsMin)(void * instance) __attribute__((ms_abi));
float HkCustomWheelConstraintData_GetAngleLimitsMin(void * instance) {
	printf("invoke HkCustomWheelConstraintData_GetAngleLimitsMin\n");
	return __PVE_HkCustomWheelConstraintData_GetAngleLimitsMin(instance);
}

float (*__PVE_HkCustomWheelConstraintData_GetAngleLimitsMax)(void * instance) __attribute__((ms_abi));
float HkCustomWheelConstraintData_GetAngleLimitsMax(void * instance) {
	printf("invoke HkCustomWheelConstraintData_GetAngleLimitsMax\n");
	return __PVE_HkCustomWheelConstraintData_GetAngleLimitsMax(instance);
}

void (*__PVE_HkCustomWheelConstraintData_DisableLimits)(void * instance) __attribute__((ms_abi));
void HkCustomWheelConstraintData_DisableLimits(void * instance) {
	printf("invoke HkCustomWheelConstraintData_DisableLimits\n");
	return __PVE_HkCustomWheelConstraintData_DisableLimits(instance);
}

float (*__PVE_HkCustomWheelConstraintData_GetCurrentAngle)(void * constraint) __attribute__((ms_abi));
float HkCustomWheelConstraintData_GetCurrentAngle(void * constraint) {
	printf("invoke HkCustomWheelConstraintData_GetCurrentAngle\n");
	return __PVE_HkCustomWheelConstraintData_GetCurrentAngle(constraint);
}

void (*__PVE_HkCustomWheelConstraintData_SetCurrentAngle)(void * constraint, float angle) __attribute__((ms_abi));
void HkCustomWheelConstraintData_SetCurrentAngle(void * constraint, float angle) {
	printf("invoke HkCustomWheelConstraintData_SetCurrentAngle\n");
	return __PVE_HkCustomWheelConstraintData_SetCurrentAngle(constraint, angle);
}

void * (*__PVE_HkFixedConstraintData_Create)() __attribute__((ms_abi));
void * HkFixedConstraintData_Create() {
	printf("invoke HkFixedConstraintData_Create\n");
	return __PVE_HkFixedConstraintData_Create();
}

void (*__PVE_HkFixedConstraintData_SetInBodySpaceInternal)(void * instance, struct Matrix pivotA, struct Matrix pivotB) __attribute__((ms_abi));
void HkFixedConstraintData_SetInBodySpaceInternal(void * instance, struct Matrix pivotA, struct Matrix pivotB) {
	printf("invoke HkFixedConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkFixedConstraintData_SetInBodySpaceInternal(instance, pivotA, pivotB);
}

void (*__PVE_HkFixedConstraintData_SetInWorldSpace)(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Matrix pivot) __attribute__((ms_abi));
void HkFixedConstraintData_SetInWorldSpace(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Matrix pivot) {
	printf("invoke HkFixedConstraintData_SetInWorldSpace\n");
	return __PVE_HkFixedConstraintData_SetInWorldSpace(instance, bodyATransform, bodyBTransform, pivot);
}

int (*__PVE_HkFixedConstraintData_IsValid)(void * instance) __attribute__((ms_abi));
int HkFixedConstraintData_IsValid(void * instance) {
	printf("invoke HkFixedConstraintData_IsValid\n");
	return __PVE_HkFixedConstraintData_IsValid(instance);
}

int (*__PVE_HkFixedConstraintData_SetInertiaStabilizationFactor)(void * instance, float value) __attribute__((ms_abi));
int HkFixedConstraintData_SetInertiaStabilizationFactor(void * instance, float value) {
	printf("invoke HkFixedConstraintData_SetInertiaStabilizationFactor\n");
	return __PVE_HkFixedConstraintData_SetInertiaStabilizationFactor(instance, value);
}

int (*__PVE_HkFixedConstraintData_GetInertiaStabilizationFactor)(void * instance, void * outResult) __attribute__((ms_abi));
int HkFixedConstraintData_GetInertiaStabilizationFactor(void * instance, void * outResult) {
	printf("invoke HkFixedConstraintData_GetInertiaStabilizationFactor\n");
	return __PVE_HkFixedConstraintData_GetInertiaStabilizationFactor(instance, outResult);
}

float (*__PVE_HkFixedConstraintData_GetSolverImpulseInLastStep)(void * constraint, char constraintAtom) __attribute__((ms_abi));
float HkFixedConstraintData_GetSolverImpulseInLastStep(void * constraint, char constraintAtom) {
	printf("invoke HkFixedConstraintData_GetSolverImpulseInLastStep\n");
	return __PVE_HkFixedConstraintData_GetSolverImpulseInLastStep(constraint, constraintAtom);
}

void * (*__PVE_HkHingeConstraintData_Create)() __attribute__((ms_abi));
void * HkHingeConstraintData_Create() {
	printf("invoke HkHingeConstraintData_Create\n");
	return __PVE_HkHingeConstraintData_Create();
}

void (*__PVE_HkHingeConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 axisA, struct Vector3 axisB) __attribute__((ms_abi));
void HkHingeConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 axisA, struct Vector3 axisB) {
	printf("invoke HkHingeConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkHingeConstraintData_SetInBodySpaceInternal(instance, pivotA, pivotB, axisA, axisB);
}

void (*__PVE_HkHingeConstraintData_SetInWorldSpace)(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Vector3 pivot, struct Vector3 axis) __attribute__((ms_abi));
void HkHingeConstraintData_SetInWorldSpace(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Vector3 pivot, struct Vector3 axis) {
	printf("invoke HkHingeConstraintData_SetInWorldSpace\n");
	return __PVE_HkHingeConstraintData_SetInWorldSpace(instance, bodyATransform, bodyBTransform, pivot, axis);
}

int (*__PVE_HkHingeConstraintData_SetInertiaStabilizationFactor)(void * instance, float value) __attribute__((ms_abi));
int HkHingeConstraintData_SetInertiaStabilizationFactor(void * instance, float value) {
	printf("invoke HkHingeConstraintData_SetInertiaStabilizationFactor\n");
	return __PVE_HkHingeConstraintData_SetInertiaStabilizationFactor(instance, value);
}

int (*__PVE_HkHingeConstraintData_GetInertiaStabilizationFactor)(void * instance, void * outResult) __attribute__((ms_abi));
int HkHingeConstraintData_GetInertiaStabilizationFactor(void * instance, void * outResult) {
	printf("invoke HkHingeConstraintData_GetInertiaStabilizationFactor\n");
	return __PVE_HkHingeConstraintData_GetInertiaStabilizationFactor(instance, outResult);
}

float (*__PVE_HkLimitedForceConstraintMotor_GetMinForce)(void * instance) __attribute__((ms_abi));
float HkLimitedForceConstraintMotor_GetMinForce(void * instance) {
	printf("invoke HkLimitedForceConstraintMotor_GetMinForce\n");
	return __PVE_HkLimitedForceConstraintMotor_GetMinForce(instance);
}

void (*__PVE_HkLimitedForceConstraintMotor_SetMinForce)(void * instance, float value) __attribute__((ms_abi));
void HkLimitedForceConstraintMotor_SetMinForce(void * instance, float value) {
	printf("invoke HkLimitedForceConstraintMotor_SetMinForce\n");
	return __PVE_HkLimitedForceConstraintMotor_SetMinForce(instance, value);
}

float (*__PVE_HkLimitedForceConstraintMotor_GetMaxForce)(void * instance) __attribute__((ms_abi));
float HkLimitedForceConstraintMotor_GetMaxForce(void * instance) {
	printf("invoke HkLimitedForceConstraintMotor_GetMaxForce\n");
	return __PVE_HkLimitedForceConstraintMotor_GetMaxForce(instance);
}

void (*__PVE_HkLimitedForceConstraintMotor_SetMaxForce)(void * instance, float value) __attribute__((ms_abi));
void HkLimitedForceConstraintMotor_SetMaxForce(void * instance, float value) {
	printf("invoke HkLimitedForceConstraintMotor_SetMaxForce\n");
	return __PVE_HkLimitedForceConstraintMotor_SetMaxForce(instance, value);
}

void * (*__PVE_HkLimitedHingeConstraintData_Create)() __attribute__((ms_abi));
void * HkLimitedHingeConstraintData_Create() {
	printf("invoke HkLimitedHingeConstraintData_Create\n");
	return __PVE_HkLimitedHingeConstraintData_Create();
}

void (*__PVE_HkLimitedHingeConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 axisA, struct Vector3 axisB, struct Vector3 axisAPerp, struct Vector3 axisBPerp) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 axisA, struct Vector3 axisB, struct Vector3 axisAPerp, struct Vector3 axisBPerp) {
	printf("invoke HkLimitedHingeConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkLimitedHingeConstraintData_SetInBodySpaceInternal(instance, pivotA, pivotB, axisA, axisB, axisAPerp, axisBPerp);
}

void (*__PVE_HkLimitedHingeConstraintData_SetInWorldSpace)(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Vector3 pivot, struct Vector3 axis) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetInWorldSpace(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Vector3 pivot, struct Vector3 axis) {
	printf("invoke HkLimitedHingeConstraintData_SetInWorldSpace\n");
	return __PVE_HkLimitedHingeConstraintData_SetInWorldSpace(instance, bodyATransform, bodyBTransform, pivot, axis);
}

void (*__PVE_HkLimitedHingeConstraintData_SetMotor)(void * instance, void * motor) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetMotor(void * instance, void * motor) {
	printf("invoke HkLimitedHingeConstraintData_SetMotor\n");
	return __PVE_HkLimitedHingeConstraintData_SetMotor(instance, motor);
}

int (*__PVE_HkLimitedHingeConstraintData_IsMotorEnabled)(void * instance) __attribute__((ms_abi));
int HkLimitedHingeConstraintData_IsMotorEnabled(void * instance) {
	printf("invoke HkLimitedHingeConstraintData_IsMotorEnabled\n");
	return __PVE_HkLimitedHingeConstraintData_IsMotorEnabled(instance);
}

void (*__PVE_HkLimitedHingeConstraintData_SetMotorEnabled)(void * instance, void * constraint, int enabled) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetMotorEnabled(void * instance, void * constraint, int enabled) {
	printf("invoke HkLimitedHingeConstraintData_SetMotorEnabled\n");
	return __PVE_HkLimitedHingeConstraintData_SetMotorEnabled(instance, constraint, enabled);
}

float (*__PVE_HkLimitedHingeConstraintData_GetTargetAngle)(void * instance) __attribute__((ms_abi));
float HkLimitedHingeConstraintData_GetTargetAngle(void * instance) {
	printf("invoke HkLimitedHingeConstraintData_GetTargetAngle\n");
	return __PVE_HkLimitedHingeConstraintData_GetTargetAngle(instance);
}

void (*__PVE_HkLimitedHingeConstraintData_SetTargetAngle)(void * instance, float value) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetTargetAngle(void * instance, float value) {
	printf("invoke HkLimitedHingeConstraintData_SetTargetAngle\n");
	return __PVE_HkLimitedHingeConstraintData_SetTargetAngle(instance, value);
}

float (*__PVE_HkLimitedHingeConstraintData_GetMaxFrictionTorque)(void * instance) __attribute__((ms_abi));
float HkLimitedHingeConstraintData_GetMaxFrictionTorque(void * instance) {
	printf("invoke HkLimitedHingeConstraintData_GetMaxFrictionTorque\n");
	return __PVE_HkLimitedHingeConstraintData_GetMaxFrictionTorque(instance);
}

void (*__PVE_HkLimitedHingeConstraintData_SetMaxFrictionTorque)(void * instance, float value) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetMaxFrictionTorque(void * instance, float value) {
	printf("invoke HkLimitedHingeConstraintData_SetMaxFrictionTorque\n");
	return __PVE_HkLimitedHingeConstraintData_SetMaxFrictionTorque(instance, value);
}

float (*__PVE_HkLimitedHingeConstraintData_GetMinAngularLimit)(void * instance) __attribute__((ms_abi));
float HkLimitedHingeConstraintData_GetMinAngularLimit(void * instance) {
	printf("invoke HkLimitedHingeConstraintData_GetMinAngularLimit\n");
	return __PVE_HkLimitedHingeConstraintData_GetMinAngularLimit(instance);
}

void (*__PVE_HkLimitedHingeConstraintData_SetMinAngularLimit)(void * instance, float value) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetMinAngularLimit(void * instance, float value) {
	printf("invoke HkLimitedHingeConstraintData_SetMinAngularLimit\n");
	return __PVE_HkLimitedHingeConstraintData_SetMinAngularLimit(instance, value);
}

float (*__PVE_HkLimitedHingeConstraintData_GetMaxAngularLimit)(void * instance) __attribute__((ms_abi));
float HkLimitedHingeConstraintData_GetMaxAngularLimit(void * instance) {
	printf("invoke HkLimitedHingeConstraintData_GetMaxAngularLimit\n");
	return __PVE_HkLimitedHingeConstraintData_GetMaxAngularLimit(instance);
}

void (*__PVE_HkLimitedHingeConstraintData_SetMaxAngularLimit)(void * instance, float value) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetMaxAngularLimit(void * instance, float value) {
	printf("invoke HkLimitedHingeConstraintData_SetMaxAngularLimit\n");
	return __PVE_HkLimitedHingeConstraintData_SetMaxAngularLimit(instance, value);
}

void (*__PVE_HkLimitedHingeConstraintData_DisableLimits)(void * instance) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_DisableLimits(void * instance) {
	printf("invoke HkLimitedHingeConstraintData_DisableLimits\n");
	return __PVE_HkLimitedHingeConstraintData_DisableLimits(instance);
}

int (*__PVE_HkLimitedHingeConstraintData_SetInertiaStabilizationFactor)(void * instance, float value) __attribute__((ms_abi));
int HkLimitedHingeConstraintData_SetInertiaStabilizationFactor(void * instance, float value) {
	printf("invoke HkLimitedHingeConstraintData_SetInertiaStabilizationFactor\n");
	return __PVE_HkLimitedHingeConstraintData_SetInertiaStabilizationFactor(instance, value);
}

int (*__PVE_HkLimitedHingeConstraintData_GetInertiaStabilizationFactor)(void * instance, void * outResult) __attribute__((ms_abi));
int HkLimitedHingeConstraintData_GetInertiaStabilizationFactor(void * instance, void * outResult) {
	printf("invoke HkLimitedHingeConstraintData_GetInertiaStabilizationFactor\n");
	return __PVE_HkLimitedHingeConstraintData_GetInertiaStabilizationFactor(instance, outResult);
}

struct Vector3 (*__PVE_HkLimitedHingeConstraintData_GetBodyAPos)(void * instance) __attribute__((ms_abi));
struct Vector3 HkLimitedHingeConstraintData_GetBodyAPos(void * instance) {
	printf("invoke HkLimitedHingeConstraintData_GetBodyAPos\n");
	return __PVE_HkLimitedHingeConstraintData_GetBodyAPos(instance);
}

struct Vector3 (*__PVE_HkLimitedHingeConstraintData_GetBodyBPos)(void * instance) __attribute__((ms_abi));
struct Vector3 HkLimitedHingeConstraintData_GetBodyBPos(void * instance) {
	printf("invoke HkLimitedHingeConstraintData_GetBodyBPos\n");
	return __PVE_HkLimitedHingeConstraintData_GetBodyBPos(instance);
}

char (*__PVE_HkLimitedHingeConstraintData_GetIsInitialized)(void * constraint) __attribute__((ms_abi));
char HkLimitedHingeConstraintData_GetIsInitialized(void * constraint) {
	printf("invoke HkLimitedHingeConstraintData_GetIsInitialized\n");
	return __PVE_HkLimitedHingeConstraintData_GetIsInitialized(constraint);
}

void (*__PVE_HkLimitedHingeConstraintData_SetIsInitialized)(void * constraint, char isInitialized) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetIsInitialized(void * constraint, char isInitialized) {
	printf("invoke HkLimitedHingeConstraintData_SetIsInitialized\n");
	return __PVE_HkLimitedHingeConstraintData_SetIsInitialized(constraint, isInitialized);
}

float (*__PVE_HkLimitedHingeConstraintData_GetPreviousTargetAngle)(void * constraint) __attribute__((ms_abi));
float HkLimitedHingeConstraintData_GetPreviousTargetAngle(void * constraint) {
	printf("invoke HkLimitedHingeConstraintData_GetPreviousTargetAngle\n");
	return __PVE_HkLimitedHingeConstraintData_GetPreviousTargetAngle(constraint);
}

void (*__PVE_HkLimitedHingeConstraintData_SetPreviousTargetAngle)(void * constraint, float previousTargetAngle) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetPreviousTargetAngle(void * constraint, float previousTargetAngle) {
	printf("invoke HkLimitedHingeConstraintData_SetPreviousTargetAngle\n");
	return __PVE_HkLimitedHingeConstraintData_SetPreviousTargetAngle(constraint, previousTargetAngle);
}

float (*__PVE_HkLimitedHingeConstraintData_GetCurrentAngle)(void * constraint) __attribute__((ms_abi));
float HkLimitedHingeConstraintData_GetCurrentAngle(void * constraint) {
	printf("invoke HkLimitedHingeConstraintData_GetCurrentAngle\n");
	return __PVE_HkLimitedHingeConstraintData_GetCurrentAngle(constraint);
}

void (*__PVE_HkLimitedHingeConstraintData_SetCurrentAngle)(void * constraint, float value) __attribute__((ms_abi));
void HkLimitedHingeConstraintData_SetCurrentAngle(void * constraint, float value) {
	printf("invoke HkLimitedHingeConstraintData_SetCurrentAngle\n");
	return __PVE_HkLimitedHingeConstraintData_SetCurrentAngle(constraint, value);
}

void * (*__PVE_HkMalleableConstraintData_Create)(void * data) __attribute__((ms_abi));
void * HkMalleableConstraintData_Create(void * data) {
	printf("invoke HkMalleableConstraintData_Create\n");
	return __PVE_HkMalleableConstraintData_Create(data);
}

float (*__PVE_HkMalleableConstraintData_GetStrength)(void * instance) __attribute__((ms_abi));
float HkMalleableConstraintData_GetStrength(void * instance) {
	printf("invoke HkMalleableConstraintData_GetStrength\n");
	return __PVE_HkMalleableConstraintData_GetStrength(instance);
}

void (*__PVE_HkMalleableConstraintData_SetStrength)(void * instance, float value) __attribute__((ms_abi));
void HkMalleableConstraintData_SetStrength(void * instance, float value) {
	printf("invoke HkMalleableConstraintData_SetStrength\n");
	return __PVE_HkMalleableConstraintData_SetStrength(instance, value);
}

void * (*__PVE_HkPrismaticConstraintData_Create)() __attribute__((ms_abi));
void * HkPrismaticConstraintData_Create() {
	printf("invoke HkPrismaticConstraintData_Create\n");
	return __PVE_HkPrismaticConstraintData_Create();
}

void (*__PVE_HkPrismaticConstraintData_SetInWorldSpace)(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Vector3 pivot, struct Vector3 axis) __attribute__((ms_abi));
void HkPrismaticConstraintData_SetInWorldSpace(void * instance, struct Matrix bodyATransform, struct Matrix bodyBTransform, struct Vector3 pivot, struct Vector3 axis) {
	printf("invoke HkPrismaticConstraintData_SetInWorldSpace\n");
	return __PVE_HkPrismaticConstraintData_SetInWorldSpace(instance, bodyATransform, bodyBTransform, pivot, axis);
}

void (*__PVE_HkPrismaticConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 bodyA, struct Vector3 bodyB, struct Vector3 axisA, struct Vector3 axisB, struct Vector3 axisAperp, struct Vector3 axisBperp) __attribute__((ms_abi));
void HkPrismaticConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 bodyA, struct Vector3 bodyB, struct Vector3 axisA, struct Vector3 axisB, struct Vector3 axisAperp, struct Vector3 axisBperp) {
	printf("invoke HkPrismaticConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkPrismaticConstraintData_SetInBodySpaceInternal(instance, bodyA, bodyB, axisA, axisB, axisAperp, axisBperp);
}

float (*__PVE_HkPrismaticConstraintData_GetMaximumLinearLimit)(void * instance) __attribute__((ms_abi));
float HkPrismaticConstraintData_GetMaximumLinearLimit(void * instance) {
	printf("invoke HkPrismaticConstraintData_GetMaximumLinearLimit\n");
	return __PVE_HkPrismaticConstraintData_GetMaximumLinearLimit(instance);
}

void (*__PVE_HkPrismaticConstraintData_SetMaximumLinearLimit)(void * instance, float value) __attribute__((ms_abi));
void HkPrismaticConstraintData_SetMaximumLinearLimit(void * instance, float value) {
	printf("invoke HkPrismaticConstraintData_SetMaximumLinearLimit\n");
	return __PVE_HkPrismaticConstraintData_SetMaximumLinearLimit(instance, value);
}

float (*__PVE_HkPrismaticConstraintData_GetMinimumLinearLimit)(void * instance) __attribute__((ms_abi));
float HkPrismaticConstraintData_GetMinimumLinearLimit(void * instance) {
	printf("invoke HkPrismaticConstraintData_GetMinimumLinearLimit\n");
	return __PVE_HkPrismaticConstraintData_GetMinimumLinearLimit(instance);
}

void (*__PVE_HkPrismaticConstraintData_SetMinimumLinearLimit)(void * instance, float value) __attribute__((ms_abi));
void HkPrismaticConstraintData_SetMinimumLinearLimit(void * instance, float value) {
	printf("invoke HkPrismaticConstraintData_SetMinimumLinearLimit\n");
	return __PVE_HkPrismaticConstraintData_SetMinimumLinearLimit(instance, value);
}

float (*__PVE_HkPrismaticConstraintData_GetMaxFrictionForce)(void * instance) __attribute__((ms_abi));
float HkPrismaticConstraintData_GetMaxFrictionForce(void * instance) {
	printf("invoke HkPrismaticConstraintData_GetMaxFrictionForce\n");
	return __PVE_HkPrismaticConstraintData_GetMaxFrictionForce(instance);
}

void (*__PVE_HkPrismaticConstraintData_SetMaxFrictionForce)(void * instance, float value) __attribute__((ms_abi));
void HkPrismaticConstraintData_SetMaxFrictionForce(void * instance, float value) {
	printf("invoke HkPrismaticConstraintData_SetMaxFrictionForce\n");
	return __PVE_HkPrismaticConstraintData_SetMaxFrictionForce(instance, value);
}

float (*__PVE_HkPrismaticConstraintData_GetTargetPosition)(void * instance) __attribute__((ms_abi));
float HkPrismaticConstraintData_GetTargetPosition(void * instance) {
	printf("invoke HkPrismaticConstraintData_GetTargetPosition\n");
	return __PVE_HkPrismaticConstraintData_GetTargetPosition(instance);
}

void (*__PVE_HkPrismaticConstraintData_SetTargetPosition)(void * instance, float value) __attribute__((ms_abi));
void HkPrismaticConstraintData_SetTargetPosition(void * instance, float value) {
	printf("invoke HkPrismaticConstraintData_SetTargetPosition\n");
	return __PVE_HkPrismaticConstraintData_SetTargetPosition(instance, value);
}

void (*__PVE_HkPrismaticConstraintData_SetMotor)(void * instance, void * motor) __attribute__((ms_abi));
void HkPrismaticConstraintData_SetMotor(void * instance, void * motor) {
	printf("invoke HkPrismaticConstraintData_SetMotor\n");
	return __PVE_HkPrismaticConstraintData_SetMotor(instance, motor);
}

int (*__PVE_HkPrismaticConstraintData_IsMotorEnabled)(void * instance) __attribute__((ms_abi));
int HkPrismaticConstraintData_IsMotorEnabled(void * instance) {
	printf("invoke HkPrismaticConstraintData_IsMotorEnabled\n");
	return __PVE_HkPrismaticConstraintData_IsMotorEnabled(instance);
}

void (*__PVE_HkPrismaticConstraintData_SetMotorEnabled)(void * instance, void * constraint, int enabled) __attribute__((ms_abi));
void HkPrismaticConstraintData_SetMotorEnabled(void * instance, void * constraint, int enabled) {
	printf("invoke HkPrismaticConstraintData_SetMotorEnabled\n");
	return __PVE_HkPrismaticConstraintData_SetMotorEnabled(instance, constraint, enabled);
}

float (*__PVE_HkPrismaticConstraintData_GetCurrentPosition)(void * constraint) __attribute__((ms_abi));
float HkPrismaticConstraintData_GetCurrentPosition(void * constraint) {
	printf("invoke HkPrismaticConstraintData_GetCurrentPosition\n");
	return __PVE_HkPrismaticConstraintData_GetCurrentPosition(constraint);
}

void * (*__PVE_HkRopeConstraintData_Create)() __attribute__((ms_abi));
void * HkRopeConstraintData_Create() {
	printf("invoke HkRopeConstraintData_Create\n");
	return __PVE_HkRopeConstraintData_Create();
}

void (*__PVE_HkRopeConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 pivotA, struct Vector3 pivotB) __attribute__((ms_abi));
void HkRopeConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 pivotA, struct Vector3 pivotB) {
	printf("invoke HkRopeConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkRopeConstraintData_SetInBodySpaceInternal(instance, pivotA, pivotB);
}

float (*__PVE_HkRopeConstraintData_Update)(void * instance, void * constraint) __attribute__((ms_abi));
float HkRopeConstraintData_Update(void * instance, void * constraint) {
	printf("invoke HkRopeConstraintData_Update\n");
	return __PVE_HkRopeConstraintData_Update(instance, constraint);
}

float (*__PVE_HkRopeConstraintData_GetStrength)(void * instance) __attribute__((ms_abi));
float HkRopeConstraintData_GetStrength(void * instance) {
	printf("invoke HkRopeConstraintData_GetStrength\n");
	return __PVE_HkRopeConstraintData_GetStrength(instance);
}

void (*__PVE_HkRopeConstraintData_SetStrength)(void * instance, float value) __attribute__((ms_abi));
void HkRopeConstraintData_SetStrength(void * instance, float value) {
	printf("invoke HkRopeConstraintData_SetStrength\n");
	return __PVE_HkRopeConstraintData_SetStrength(instance, value);
}

float (*__PVE_HkRopeConstraintData_GetLinearLimit)(void * instance) __attribute__((ms_abi));
float HkRopeConstraintData_GetLinearLimit(void * instance) {
	printf("invoke HkRopeConstraintData_GetLinearLimit\n");
	return __PVE_HkRopeConstraintData_GetLinearLimit(instance);
}

void (*__PVE_HkRopeConstraintData_SetLinearLimit)(void * instance, float value) __attribute__((ms_abi));
void HkRopeConstraintData_SetLinearLimit(void * instance, float value) {
	printf("invoke HkRopeConstraintData_SetLinearLimit\n");
	return __PVE_HkRopeConstraintData_SetLinearLimit(instance, value);
}

int (*__PVE_HkRopeConstraintData_IsValid)(void * instance) __attribute__((ms_abi));
int HkRopeConstraintData_IsValid(void * instance) {
	printf("invoke HkRopeConstraintData_IsValid\n");
	return __PVE_HkRopeConstraintData_IsValid(instance);
}

void * (*__PVE_HkVelocityConstraintMotor_Create)(float velocityTarget, float maxForce) __attribute__((ms_abi));
void * HkVelocityConstraintMotor_Create(float velocityTarget, float maxForce) {
	printf("invoke HkVelocityConstraintMotor_Create\n");
	return __PVE_HkVelocityConstraintMotor_Create(velocityTarget, maxForce);
}

float (*__PVE_HkVelocityConstraintMotor_GetTau)(void * instance) __attribute__((ms_abi));
float HkVelocityConstraintMotor_GetTau(void * instance) {
	printf("invoke HkVelocityConstraintMotor_GetTau\n");
	return __PVE_HkVelocityConstraintMotor_GetTau(instance);
}

void (*__PVE_HkVelocityConstraintMotor_SetTau)(void * instance, float value) __attribute__((ms_abi));
void HkVelocityConstraintMotor_SetTau(void * instance, float value) {
	printf("invoke HkVelocityConstraintMotor_SetTau\n");
	return __PVE_HkVelocityConstraintMotor_SetTau(instance, value);
}

float (*__PVE_HkVelocityConstraintMotor_GetVelocityTarget)(void * instance) __attribute__((ms_abi));
float HkVelocityConstraintMotor_GetVelocityTarget(void * instance) {
	printf("invoke HkVelocityConstraintMotor_GetVelocityTarget\n");
	return __PVE_HkVelocityConstraintMotor_GetVelocityTarget(instance);
}

void (*__PVE_HkVelocityConstraintMotor_SetVelocityTarget)(void * instance, float value) __attribute__((ms_abi));
void HkVelocityConstraintMotor_SetVelocityTarget(void * instance, float value) {
	printf("invoke HkVelocityConstraintMotor_SetVelocityTarget\n");
	return __PVE_HkVelocityConstraintMotor_SetVelocityTarget(instance, value);
}

int (*__PVE_HkVelocityConstraintMotor_GetConstantRecoveryVelocity)(void * instance) __attribute__((ms_abi));
int HkVelocityConstraintMotor_GetConstantRecoveryVelocity(void * instance) {
	printf("invoke HkVelocityConstraintMotor_GetConstantRecoveryVelocity\n");
	return __PVE_HkVelocityConstraintMotor_GetConstantRecoveryVelocity(instance);
}

void (*__PVE_HkVelocityConstraintMotor_SetConstantRecoveryVelocity)(void * instance, int value) __attribute__((ms_abi));
void HkVelocityConstraintMotor_SetConstantRecoveryVelocity(void * instance, int value) {
	printf("invoke HkVelocityConstraintMotor_SetConstantRecoveryVelocity\n");
	return __PVE_HkVelocityConstraintMotor_SetConstantRecoveryVelocity(instance, value);
}

void * (*__PVE_HkWheelConstraintData_Create)() __attribute__((ms_abi));
void * HkWheelConstraintData_Create() {
	printf("invoke HkWheelConstraintData_Create\n");
	return __PVE_HkWheelConstraintData_Create();
}

void (*__PVE_HkWheelConstraintData_SetInWorldSpace)(void * instance, struct Matrix wheelBody, struct Matrix chasisBody, struct Vector3 pivot, struct Vector3 axle, struct Vector3 suspensionAxis, struct Vector3 steeringAxis) __attribute__((ms_abi));
void HkWheelConstraintData_SetInWorldSpace(void * instance, struct Matrix wheelBody, struct Matrix chasisBody, struct Vector3 pivot, struct Vector3 axle, struct Vector3 suspensionAxis, struct Vector3 steeringAxis) {
	printf("invoke HkWheelConstraintData_SetInWorldSpace\n");
	return __PVE_HkWheelConstraintData_SetInWorldSpace(instance, wheelBody, chasisBody, pivot, axle, suspensionAxis, steeringAxis);
}

void (*__PVE_HkWheelConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 axleA, struct Vector3 axleB, struct Vector3 suspensionAxisB, struct Vector3 steeringAxisB) __attribute__((ms_abi));
void HkWheelConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 axleA, struct Vector3 axleB, struct Vector3 suspensionAxisB, struct Vector3 steeringAxisB) {
	printf("invoke HkWheelConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkWheelConstraintData_SetInBodySpaceInternal(instance, pivotA, pivotB, axleA, axleB, suspensionAxisB, steeringAxisB);
}

void (*__PVE_HkWheelConstraintData_SetSuspensionMinLimit)(void * instance, float value) __attribute__((ms_abi));
void HkWheelConstraintData_SetSuspensionMinLimit(void * instance, float value) {
	printf("invoke HkWheelConstraintData_SetSuspensionMinLimit\n");
	return __PVE_HkWheelConstraintData_SetSuspensionMinLimit(instance, value);
}

void (*__PVE_HkWheelConstraintData_SetSuspensionMaxLimit)(void * instance, float value) __attribute__((ms_abi));
void HkWheelConstraintData_SetSuspensionMaxLimit(void * instance, float value) {
	printf("invoke HkWheelConstraintData_SetSuspensionMaxLimit\n");
	return __PVE_HkWheelConstraintData_SetSuspensionMaxLimit(instance, value);
}

void (*__PVE_HkWheelConstraintData_SetSuspensionStrength)(void * instance, float value) __attribute__((ms_abi));
void HkWheelConstraintData_SetSuspensionStrength(void * instance, float value) {
	printf("invoke HkWheelConstraintData_SetSuspensionStrength\n");
	return __PVE_HkWheelConstraintData_SetSuspensionStrength(instance, value);
}

void (*__PVE_HkWheelConstraintData_SetSuspensionDamping)(void * instance, float value) __attribute__((ms_abi));
void HkWheelConstraintData_SetSuspensionDamping(void * instance, float value) {
	printf("invoke HkWheelConstraintData_SetSuspensionDamping\n");
	return __PVE_HkWheelConstraintData_SetSuspensionDamping(instance, value);
}

void (*__PVE_HkWheelConstraintData_SetSteeringAngle)(void * instance, float value) __attribute__((ms_abi));
void HkWheelConstraintData_SetSteeringAngle(void * instance, float value) {
	printf("invoke HkWheelConstraintData_SetSteeringAngle\n");
	return __PVE_HkWheelConstraintData_SetSteeringAngle(instance, value);
}

void * (*__PVE_HkdDecomposeFracture_Create)() __attribute__((ms_abi));
void * HkdDecomposeFracture_Create() {
	printf("invoke HkdDecomposeFracture_Create\n");
	return __PVE_HkdDecomposeFracture_Create();
}

float (*__PVE_HkdDecomposeFracture_GetClipZoneWidth)(void * instance) __attribute__((ms_abi));
float HkdDecomposeFracture_GetClipZoneWidth(void * instance) {
	printf("invoke HkdDecomposeFracture_GetClipZoneWidth\n");
	return __PVE_HkdDecomposeFracture_GetClipZoneWidth(instance);
}

void (*__PVE_HkdDecomposeFracture_SetClipZoneWidth)(void * instance, float clipZoneWidth) __attribute__((ms_abi));
void HkdDecomposeFracture_SetClipZoneWidth(void * instance, float clipZoneWidth) {
	printf("invoke HkdDecomposeFracture_SetClipZoneWidth\n");
	return __PVE_HkdDecomposeFracture_SetClipZoneWidth(instance, clipZoneWidth);
}

float (*__PVE_HkdDecomposeFracture_GetShiftToSmallerCrossSection)(void * instance) __attribute__((ms_abi));
float HkdDecomposeFracture_GetShiftToSmallerCrossSection(void * instance) {
	printf("invoke HkdDecomposeFracture_GetShiftToSmallerCrossSection\n");
	return __PVE_HkdDecomposeFracture_GetShiftToSmallerCrossSection(instance);
}

void (*__PVE_HkdDecomposeFracture_SetShiftToSmallerCrossSection)(void * instance, float shiftToSmallerCrossSection) __attribute__((ms_abi));
void HkdDecomposeFracture_SetShiftToSmallerCrossSection(void * instance, float shiftToSmallerCrossSection) {
	printf("invoke HkdDecomposeFracture_SetShiftToSmallerCrossSection\n");
	return __PVE_HkdDecomposeFracture_SetShiftToSmallerCrossSection(instance, shiftToSmallerCrossSection);
}

void (*__PVE_HkdDecomposeFracture_SetGeometry)(void * instance, void * geometry) __attribute__((ms_abi));
void HkdDecomposeFracture_SetGeometry(void * instance, void * geometry) {
	printf("invoke HkdDecomposeFracture_SetGeometry\n");
	return __PVE_HkdDecomposeFracture_SetGeometry(instance, geometry);
}

int (*__PVE_HkdFracture_GetFlattenHierarchy)(void * instance) __attribute__((ms_abi));
int HkdFracture_GetFlattenHierarchy(void * instance) {
	printf("invoke HkdFracture_GetFlattenHierarchy\n");
	return __PVE_HkdFracture_GetFlattenHierarchy(instance);
}

void (*__PVE_HkdFracture_SetFlattenHierarchy)(void * instance, int flattenHierarchy) __attribute__((ms_abi));
void HkdFracture_SetFlattenHierarchy(void * instance, int flattenHierarchy) {
	printf("invoke HkdFracture_SetFlattenHierarchy\n");
	return __PVE_HkdFracture_SetFlattenHierarchy(instance, flattenHierarchy);
}

int (*__PVE_HkdFracture_GetRefitType)(void * instance) __attribute__((ms_abi));
int HkdFracture_GetRefitType(void * instance) {
	printf("invoke HkdFracture_GetRefitType\n");
	return __PVE_HkdFracture_GetRefitType(instance);
}

void (*__PVE_HkdFracture_SetRefitType)(void * instance, int refitType) __attribute__((ms_abi));
void HkdFracture_SetRefitType(void * instance, int refitType) {
	printf("invoke HkdFracture_SetRefitType\n");
	return __PVE_HkdFracture_SetRefitType(instance, refitType);
}

void * (*__PVE_HkdRandomSplitFracture_Create)() __attribute__((ms_abi));
void * HkdRandomSplitFracture_Create() {
	printf("invoke HkdRandomSplitFracture_Create\n");
	return __PVE_HkdRandomSplitFracture_Create();
}

void * (*__PVE_HkdRandomSplitFracture_ReCast)(void * instance) __attribute__((ms_abi));
void * HkdRandomSplitFracture_ReCast(void * instance) {
	printf("invoke HkdRandomSplitFracture_ReCast\n");
	return __PVE_HkdRandomSplitFracture_ReCast(instance);
}

float (*__PVE_HkdRandomSplitFracture_GetRandomRange)(void * instance) __attribute__((ms_abi));
float HkdRandomSplitFracture_GetRandomRange(void * instance) {
	printf("invoke HkdRandomSplitFracture_GetRandomRange\n");
	return __PVE_HkdRandomSplitFracture_GetRandomRange(instance);
}

void (*__PVE_HkdRandomSplitFracture_SetRandomRange)(void * instance, float randomRange) __attribute__((ms_abi));
void HkdRandomSplitFracture_SetRandomRange(void * instance, float randomRange) {
	printf("invoke HkdRandomSplitFracture_SetRandomRange\n");
	return __PVE_HkdRandomSplitFracture_SetRandomRange(instance, randomRange);
}

struct Vector4 (*__PVE_HkdRandomSplitFracture_GetSplitGeometryScale)(void * instance) __attribute__((ms_abi));
struct Vector4 HkdRandomSplitFracture_GetSplitGeometryScale(void * instance) {
	printf("invoke HkdRandomSplitFracture_GetSplitGeometryScale\n");
	return __PVE_HkdRandomSplitFracture_GetSplitGeometryScale(instance);
}

void (*__PVE_HkdRandomSplitFracture_SetSplitGeometryScale)(void * instance, struct Vector4 splitGeometryScale) __attribute__((ms_abi));
void HkdRandomSplitFracture_SetSplitGeometryScale(void * instance, struct Vector4 splitGeometryScale) {
	printf("invoke HkdRandomSplitFracture_SetSplitGeometryScale\n");
	return __PVE_HkdRandomSplitFracture_SetSplitGeometryScale(instance, splitGeometryScale);
}

int (*__PVE_HkdRandomSplitFracture_GetSplitLargestVolumesFirst)(void * instance) __attribute__((ms_abi));
int HkdRandomSplitFracture_GetSplitLargestVolumesFirst(void * instance) {
	printf("invoke HkdRandomSplitFracture_GetSplitLargestVolumesFirst\n");
	return __PVE_HkdRandomSplitFracture_GetSplitLargestVolumesFirst(instance);
}

void (*__PVE_HkdRandomSplitFracture_SetSplitLargestVolumesFirst)(void * instance, int splitLargestVolumesFirst) __attribute__((ms_abi));
void HkdRandomSplitFracture_SetSplitLargestVolumesFirst(void * instance, int splitLargestVolumesFirst) {
	printf("invoke HkdRandomSplitFracture_SetSplitLargestVolumesFirst\n");
	return __PVE_HkdRandomSplitFracture_SetSplitLargestVolumesFirst(instance, splitLargestVolumesFirst);
}

int (*__PVE_HkdRandomSplitFracture_GetRandomSeed)(void * instance, int index) __attribute__((ms_abi));
int HkdRandomSplitFracture_GetRandomSeed(void * instance, int index) {
	printf("invoke HkdRandomSplitFracture_GetRandomSeed\n");
	return __PVE_HkdRandomSplitFracture_GetRandomSeed(instance, index);
}

void (*__PVE_HkdRandomSplitFracture_SetRandomSeed)(void * instance, int index, int randomSeed) __attribute__((ms_abi));
void HkdRandomSplitFracture_SetRandomSeed(void * instance, int index, int randomSeed) {
	printf("invoke HkdRandomSplitFracture_SetRandomSeed\n");
	return __PVE_HkdRandomSplitFracture_SetRandomSeed(instance, index, randomSeed);
}

int (*__PVE_HkdRandomSplitFracture_GetNumObjectsOnLevel)(void * instance, int index) __attribute__((ms_abi));
int HkdRandomSplitFracture_GetNumObjectsOnLevel(void * instance, int index) {
	printf("invoke HkdRandomSplitFracture_GetNumObjectsOnLevel\n");
	return __PVE_HkdRandomSplitFracture_GetNumObjectsOnLevel(instance, index);
}

void (*__PVE_HkdRandomSplitFracture_SetNumObjectsOnLevel)(void * instance, int index, int numObjectsOnLevel) __attribute__((ms_abi));
void HkdRandomSplitFracture_SetNumObjectsOnLevel(void * instance, int index, int numObjectsOnLevel) {
	printf("invoke HkdRandomSplitFracture_SetNumObjectsOnLevel\n");
	return __PVE_HkdRandomSplitFracture_SetNumObjectsOnLevel(instance, index, numObjectsOnLevel);
}

void (*__PVE_HkdRandomSplitFracture_SetGeometry)(void * instance, void * geometry) __attribute__((ms_abi));
void HkdRandomSplitFracture_SetGeometry(void * instance, void * geometry) {
	printf("invoke HkdRandomSplitFracture_SetGeometry\n");
	return __PVE_HkdRandomSplitFracture_SetGeometry(instance, geometry);
}

void * (*__PVE_HkdVoronoiFracture_Create)() __attribute__((ms_abi));
void * HkdVoronoiFracture_Create() {
	printf("invoke HkdVoronoiFracture_Create\n");
	return __PVE_HkdVoronoiFracture_Create();
}

int (*__PVE_HkdVoronoiFracture_GetNumIterations)(void * instance) __attribute__((ms_abi));
int HkdVoronoiFracture_GetNumIterations(void * instance) {
	printf("invoke HkdVoronoiFracture_GetNumIterations\n");
	return __PVE_HkdVoronoiFracture_GetNumIterations(instance);
}

void (*__PVE_HkdVoronoiFracture_SetNumIterations)(void * instance, int numIterations) __attribute__((ms_abi));
void HkdVoronoiFracture_SetNumIterations(void * instance, int numIterations) {
	printf("invoke HkdVoronoiFracture_SetNumIterations\n");
	return __PVE_HkdVoronoiFracture_SetNumIterations(instance, numIterations);
}

int (*__PVE_HkdVoronoiFracture_GetNumSitesToGenerate)(void * instance) __attribute__((ms_abi));
int HkdVoronoiFracture_GetNumSitesToGenerate(void * instance) {
	printf("invoke HkdVoronoiFracture_GetNumSitesToGenerate\n");
	return __PVE_HkdVoronoiFracture_GetNumSitesToGenerate(instance);
}

void (*__PVE_HkdVoronoiFracture_SetNumSitesToGenerate)(void * instance, int numSitesToGenerate) __attribute__((ms_abi));
void HkdVoronoiFracture_SetNumSitesToGenerate(void * instance, int numSitesToGenerate) {
	printf("invoke HkdVoronoiFracture_SetNumSitesToGenerate\n");
	return __PVE_HkdVoronoiFracture_SetNumSitesToGenerate(instance, numSitesToGenerate);
}

int (*__PVE_HkdVoronoiFracture_GetSeed)(void * instance) __attribute__((ms_abi));
int HkdVoronoiFracture_GetSeed(void * instance) {
	printf("invoke HkdVoronoiFracture_GetSeed\n");
	return __PVE_HkdVoronoiFracture_GetSeed(instance);
}

void (*__PVE_HkdVoronoiFracture_SetSeed)(void * instance, int seed) __attribute__((ms_abi));
void HkdVoronoiFracture_SetSeed(void * instance, int seed) {
	printf("invoke HkdVoronoiFracture_SetSeed\n");
	return __PVE_HkdVoronoiFracture_SetSeed(instance, seed);
}

void (*__PVE_HkdVoronoiFracture_SetGeometry)(void * instance, void * geometry) __attribute__((ms_abi));
void HkdVoronoiFracture_SetGeometry(void * instance, void * geometry) {
	printf("invoke HkdVoronoiFracture_SetGeometry\n");
	return __PVE_HkdVoronoiFracture_SetGeometry(instance, geometry);
}

void * (*__PVE_HkdWoodFracture_Create)() __attribute__((ms_abi));
void * HkdWoodFracture_Create() {
	printf("invoke HkdWoodFracture_Create\n");
	return __PVE_HkdWoodFracture_Create();
}

void * (*__PVE_HkdWoodFracture_ReCast)(void * instance) __attribute__((ms_abi));
void * HkdWoodFracture_ReCast(void * instance) {
	printf("invoke HkdWoodFracture_ReCast\n");
	return __PVE_HkdWoodFracture_ReCast(instance);
}

void * (*__PVE_HkdWoodFracture_GetSplinterSplittingGeometry)(void * instance) __attribute__((ms_abi));
void * HkdWoodFracture_GetSplinterSplittingGeometry(void * instance) {
	printf("invoke HkdWoodFracture_GetSplinterSplittingGeometry\n");
	return __PVE_HkdWoodFracture_GetSplinterSplittingGeometry(instance);
}

void (*__PVE_HkdWoodFracture_SetSplinterSplittingGeometry)(void * instance, void * geometry) __attribute__((ms_abi));
void HkdWoodFracture_SetSplinterSplittingGeometry(void * instance, void * geometry) {
	printf("invoke HkdWoodFracture_SetSplinterSplittingGeometry\n");
	return __PVE_HkdWoodFracture_SetSplinterSplittingGeometry(instance, geometry);
}

void * (*__PVE_HkdWoodFracture_GetBoardSplittingGeometry)(void * instance) __attribute__((ms_abi));
void * HkdWoodFracture_GetBoardSplittingGeometry(void * instance) {
	printf("invoke HkdWoodFracture_GetBoardSplittingGeometry\n");
	return __PVE_HkdWoodFracture_GetBoardSplittingGeometry(instance);
}

void (*__PVE_HkdWoodFracture_SetBoardSplittingGeometry)(void * instance, void * geometry) __attribute__((ms_abi));
void HkdWoodFracture_SetBoardSplittingGeometry(void * instance, void * geometry) {
	printf("invoke HkdWoodFracture_SetBoardSplittingGeometry\n");
	return __PVE_HkdWoodFracture_SetBoardSplittingGeometry(instance, geometry);
}

struct SplittingData (*__PVE_HkdWoodFracture_GetSplinterSplittingData)(void * instance) __attribute__((ms_abi));
struct SplittingData HkdWoodFracture_GetSplinterSplittingData(void * instance) {
	printf("invoke HkdWoodFracture_GetSplinterSplittingData\n");
	return __PVE_HkdWoodFracture_GetSplinterSplittingData(instance);
}

void (*__PVE_HkdWoodFracture_SetSplinterSplittingData)(void * instance, struct SplittingData data) __attribute__((ms_abi));
void HkdWoodFracture_SetSplinterSplittingData(void * instance, struct SplittingData data) {
	printf("invoke HkdWoodFracture_SetSplinterSplittingData\n");
	return __PVE_HkdWoodFracture_SetSplinterSplittingData(instance, data);
}

struct SplittingData (*__PVE_HkdWoodFracture_GetBoardSplittingData)(void * instance) __attribute__((ms_abi));
struct SplittingData HkdWoodFracture_GetBoardSplittingData(void * instance) {
	printf("invoke HkdWoodFracture_GetBoardSplittingData\n");
	return __PVE_HkdWoodFracture_GetBoardSplittingData(instance);
}

void (*__PVE_HkdWoodFracture_SetBoardSplittingData)(void * instance, struct SplittingData data) __attribute__((ms_abi));
void HkdWoodFracture_SetBoardSplittingData(void * instance, struct SplittingData data) {
	printf("invoke HkdWoodFracture_SetBoardSplittingData\n");
	return __PVE_HkdWoodFracture_SetBoardSplittingData(instance, data);
}

int (*__PVE_HkdWoodFracture_GetRandomSeed)(void * instance) __attribute__((ms_abi));
int HkdWoodFracture_GetRandomSeed(void * instance) {
	printf("invoke HkdWoodFracture_GetRandomSeed\n");
	return __PVE_HkdWoodFracture_GetRandomSeed(instance);
}

void (*__PVE_HkdWoodFracture_SetRandomSeed)(void * instance, int data) __attribute__((ms_abi));
void HkdWoodFracture_SetRandomSeed(void * instance, int data) {
	printf("invoke HkdWoodFracture_SetRandomSeed\n");
	return __PVE_HkdWoodFracture_SetRandomSeed(instance, data);
}

void * (*__PVE_HkBreakOffPartsUtil_Create)(void * breakLogicHandler, void * breakPartsHandler) __attribute__((ms_abi));
void * HkBreakOffPartsUtil_Create(void * breakLogicHandler, void * breakPartsHandler) {
	printf("invoke HkBreakOffPartsUtil_Create\n");
	return __PVE_HkBreakOffPartsUtil_Create(_PVE_Trampoline_Havok_HkBreakOffPartsUtil_BreakLogicHandlerDelegate(breakLogicHandler), _PVE_Trampoline_Havok_HkBreakOffPartsUtil_BreakPartsHandlerDelegate(breakPartsHandler));
}

void (*__PVE_HkBreakOffPartsUtil_Release)(void * instance) __attribute__((ms_abi));
void HkBreakOffPartsUtil_Release(void * instance) {
	printf("invoke HkBreakOffPartsUtil_Release\n");
	return __PVE_HkBreakOffPartsUtil_Release(instance);
}

void (*__PVE_HkBreakOffPartsUtil_RemoveKeysFromListShape)(void * entity, void * shapeKeys, int count) __attribute__((ms_abi));
void HkBreakOffPartsUtil_RemoveKeysFromListShape(void * entity, void * shapeKeys, int count) {
	printf("invoke HkBreakOffPartsUtil_RemoveKeysFromListShape\n");
	return __PVE_HkBreakOffPartsUtil_RemoveKeysFromListShape(entity, shapeKeys, count);
}

void (*__PVE_HkBreakOffPartsUtil_MarkEntityBreakable)(void * instance, void * entity, float maxImpulse) __attribute__((ms_abi));
void HkBreakOffPartsUtil_MarkEntityBreakable(void * instance, void * entity, float maxImpulse) {
	printf("invoke HkBreakOffPartsUtil_MarkEntityBreakable\n");
	return __PVE_HkBreakOffPartsUtil_MarkEntityBreakable(instance, entity, maxImpulse);
}

void (*__PVE_HkBreakOffPartsUtil_MarkPieceBreakable)(void * instance, void * entity, int shapeKey, float maxImpulse) __attribute__((ms_abi));
void HkBreakOffPartsUtil_MarkPieceBreakable(void * instance, void * entity, int shapeKey, float maxImpulse) {
	printf("invoke HkBreakOffPartsUtil_MarkPieceBreakable\n");
	return __PVE_HkBreakOffPartsUtil_MarkPieceBreakable(instance, entity, shapeKey, maxImpulse);
}

void (*__PVE_HkBreakOffPartsUtil_SetMaxConstraintImpulse)(void * instance, void * entity, float maxConstraintImpulse) __attribute__((ms_abi));
void HkBreakOffPartsUtil_SetMaxConstraintImpulse(void * instance, void * entity, float maxConstraintImpulse) {
	printf("invoke HkBreakOffPartsUtil_SetMaxConstraintImpulse\n");
	return __PVE_HkBreakOffPartsUtil_SetMaxConstraintImpulse(instance, entity, maxConstraintImpulse);
}

void (*__PVE_HkBreakOffPartsUtil_UnmarkEntityBreakable)(void * instance, void * entity) __attribute__((ms_abi));
void HkBreakOffPartsUtil_UnmarkEntityBreakable(void * instance, void * entity) {
	printf("invoke HkBreakOffPartsUtil_UnmarkEntityBreakable\n");
	return __PVE_HkBreakOffPartsUtil_UnmarkEntityBreakable(instance, entity);
}

void (*__PVE_HkBreakOffPartsUtil_UnmarkPieceBreakable)(void * instance, void * entity, int shapeKey) __attribute__((ms_abi));
void HkBreakOffPartsUtil_UnmarkPieceBreakable(void * instance, void * entity, int shapeKey) {
	printf("invoke HkBreakOffPartsUtil_UnmarkPieceBreakable\n");
	return __PVE_HkBreakOffPartsUtil_UnmarkPieceBreakable(instance, entity, shapeKey);
}

int (*__PVE_HkBreakOffPoints_Count)(void * instance) __attribute__((ms_abi));
int HkBreakOffPoints_Count(void * instance) {
	printf("invoke HkBreakOffPoints_Count\n");
	return __PVE_HkBreakOffPoints_Count(instance);
}

void (*__PVE_HkBreakOffPoints_Get)(void * instance, int index, void * outPointInfo) __attribute__((ms_abi));
void HkBreakOffPoints_Get(void * instance, int index, void * outPointInfo) {
	printf("invoke HkBreakOffPoints_Get\n");
	return __PVE_HkBreakOffPoints_Get(instance, index, outPointInfo);
}

void * (*__PVE_HkdBreakableBody_Create)(void * breakableShape, void * body, void * world, struct Matrix matrix) __attribute__((ms_abi));
void * HkdBreakableBody_Create(void * breakableShape, void * body, void * world, struct Matrix matrix) {
	printf("invoke HkdBreakableBody_Create\n");
	return __PVE_HkdBreakableBody_Create(breakableShape, body, world, matrix);
}

void * (*__PVE_HkdBreakableBody_InitListener)(void * instance, void * beforeReplaceBody, void * afterReplaceBody, void * bodyAddedToWorld, void * beforeControllerOperation, void * afterControllerOperation) __attribute__((ms_abi));
void * HkdBreakableBody_InitListener(void * instance, void * beforeReplaceBody, void * afterReplaceBody, void * bodyAddedToWorld, void * beforeControllerOperation, void * afterControllerOperation) {
	printf("invoke HkdBreakableBody_InitListener\n");
	return __PVE_HkdBreakableBody_InitListener(instance, _PVE_Trampoline_Havok_HkdBreakableBody_CallBodyReplacedEvent(beforeReplaceBody), _PVE_Trampoline_Havok_HkdBreakableBody_CallBodyReplacedEvent(afterReplaceBody), _PVE_Trampoline_Havok_HkdBreakableBody_CallBreakableBodyEvent(bodyAddedToWorld), _PVE_Trampoline_Havok_HkdBreakableBody_CallBreakableBodyEvent(beforeControllerOperation), _PVE_Trampoline_Havok_HkdBreakableBody_CallBreakableBodyEvent(afterControllerOperation));
}

void * (*__PVE_HkdBreakableBody_GetBreakableShape)(void * instance) __attribute__((ms_abi));
void * HkdBreakableBody_GetBreakableShape(void * instance) {
	printf("invoke HkdBreakableBody_GetBreakableShape\n");
	return __PVE_HkdBreakableBody_GetBreakableShape(instance);
}

void (*__PVE_HkdBreakableBody_SetBreakableShape)(void * instance, void * breakableShape) __attribute__((ms_abi));
void HkdBreakableBody_SetBreakableShape(void * instance, void * breakableShape) {
	printf("invoke HkdBreakableBody_SetBreakableShape\n");
	return __PVE_HkdBreakableBody_SetBreakableShape(instance, breakableShape);
}

void (*__PVE_HkdBreakableBody_Clear)(void * instance, void * listener) __attribute__((ms_abi));
void HkdBreakableBody_Clear(void * instance, void * listener) {
	printf("invoke HkdBreakableBody_Clear\n");
	return __PVE_HkdBreakableBody_Clear(instance, listener);
}

int (*__PVE_HkdBreakableBody_ConnectToWorld)(void * instance, void * world, float distance) __attribute__((ms_abi));
int HkdBreakableBody_ConnectToWorld(void * instance, void * world, float distance) {
	printf("invoke HkdBreakableBody_ConnectToWorld\n");
	return __PVE_HkdBreakableBody_ConnectToWorld(instance, world, distance);
}

void * (*__PVE_HkdBreakableBody_GetRigidBody)(void * instance) __attribute__((ms_abi));
void * HkdBreakableBody_GetRigidBody(void * instance) {
	printf("invoke HkdBreakableBody_GetRigidBody\n");
	return __PVE_HkdBreakableBody_GetRigidBody(instance);
}

void * (*__PVE_HkdBreakableBody_Initialize)(void * bInfo, void * rigidBody) __attribute__((ms_abi));
void * HkdBreakableBody_Initialize(void * bInfo, void * rigidBody) {
	printf("invoke HkdBreakableBody_Initialize\n");
	return __PVE_HkdBreakableBody_Initialize(bInfo, rigidBody);
}

void (*__PVE_HkdBreakableBody_RemoveConnection)(void * instance, void * connection) __attribute__((ms_abi));
void HkdBreakableBody_RemoveConnection(void * instance, void * connection) {
	printf("invoke HkdBreakableBody_RemoveConnection\n");
	return __PVE_HkdBreakableBody_RemoveConnection(instance, connection);
}

void (*__PVE_HkdBreakableBody_SetFixedConnectivity)(void * instance, void * connectivity) __attribute__((ms_abi));
void HkdBreakableBody_SetFixedConnectivity(void * instance, void * connectivity) {
	printf("invoke HkdBreakableBody_SetFixedConnectivity\n");
	return __PVE_HkdBreakableBody_SetFixedConnectivity(instance, connectivity);
}

void (*__PVE_HkdBreakableBodyHelper_GetChildren)(void * instance, void * returnShapeInstanceInfo) __attribute__((ms_abi));
void HkdBreakableBodyHelper_GetChildren(void * instance, void * returnShapeInstanceInfo) {
	printf("invoke HkdBreakableBodyHelper_GetChildren\n");
	return __PVE_HkdBreakableBodyHelper_GetChildren(instance, _PVE_Trampoline_Havok_HkdBreakableBodyHelper_ReturnShapeInstanceInfo(returnShapeInstanceInfo));
}

struct Matrix (*__PVE_HkdBreakableBodyHelper_GetRigidBodyMatrix)(void * instance) __attribute__((ms_abi));
struct Matrix HkdBreakableBodyHelper_GetRigidBodyMatrix(void * instance) {
	printf("invoke HkdBreakableBodyHelper_GetRigidBodyMatrix\n");
	return __PVE_HkdBreakableBodyHelper_GetRigidBodyMatrix(instance);
}

struct Vector3 (*__PVE_HkdBreakableBodyHelper_GetShapeCoM)(void * instance) __attribute__((ms_abi));
struct Vector3 HkdBreakableBodyHelper_GetShapeCoM(void * instance) {
	printf("invoke HkdBreakableBodyHelper_GetShapeCoM\n");
	return __PVE_HkdBreakableBodyHelper_GetShapeCoM(instance);
}

void * (*__PVE_HkdBreakableBodyInfo_GetBody)(void * instance) __attribute__((ms_abi));
void * HkdBreakableBodyInfo_GetBody(void * instance) {
	printf("invoke HkdBreakableBodyInfo_GetBody\n");
	return __PVE_HkdBreakableBodyInfo_GetBody(instance);
}

int (*__PVE_HkdBreakableBodyInfo_IsFracture)(void * instance) __attribute__((ms_abi));
int HkdBreakableBodyInfo_IsFracture(void * instance) {
	printf("invoke HkdBreakableBodyInfo_IsFracture\n");
	return __PVE_HkdBreakableBodyInfo_IsFracture(instance);
}

void * (*__PVE_HkdBreakableShape_Create)(void * shape) __attribute__((ms_abi));
void * HkdBreakableShape_Create(void * shape) {
	printf("invoke HkdBreakableShape_Create\n");
	return __PVE_HkdBreakableShape_Create(shape);
}

void * (*__PVE_HkdBreakableShape_CreateWithMass)(void * shape, struct HkMassProperties massProps) __attribute__((ms_abi));
void * HkdBreakableShape_CreateWithMass(void * shape, struct HkMassProperties massProps) {
	printf("invoke HkdBreakableShape_CreateWithMass\n");
	return __PVE_HkdBreakableShape_CreateWithMass(shape, massProps);
}

void * (*__PVE_HkdBreakableShape_GetShapeName)(void * instance) __attribute__((ms_abi));
void * HkdBreakableShape_GetShapeName(void * instance) {
	printf("invoke HkdBreakableShape_GetShapeName\n");
	return __PVE_HkdBreakableShape_GetShapeName(instance);
}

int (*__PVE_HkdBreakableShape_GetMaterialType)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_GetMaterialType(void * instance) {
	printf("invoke HkdBreakableShape_GetMaterialType\n");
	return __PVE_HkdBreakableShape_GetMaterialType(instance);
}

int (*__PVE_HkdBreakableShape_GetMotionQuality)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_GetMotionQuality(void * instance) {
	printf("invoke HkdBreakableShape_GetMotionQuality\n");
	return __PVE_HkdBreakableShape_GetMotionQuality(instance);
}

void (*__PVE_HkdBreakableShape_SetMotionQuality)(void * instance, int motionQuality) __attribute__((ms_abi));
void HkdBreakableShape_SetMotionQuality(void * instance, int motionQuality) {
	printf("invoke HkdBreakableShape_SetMotionQuality\n");
	return __PVE_HkdBreakableShape_SetMotionQuality(instance, motionQuality);
}

int (*__PVE_HkdBreakableShape_GetHasParent)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_GetHasParent(void * instance) {
	printf("invoke HkdBreakableShape_GetHasParent\n");
	return __PVE_HkdBreakableShape_GetHasParent(instance);
}

void * (*__PVE_HkdBreakableShape_GetName)(void * instance) __attribute__((ms_abi));
void * HkdBreakableShape_GetName(void * instance) {
	printf("invoke HkdBreakableShape_GetName\n");
	return __PVE_HkdBreakableShape_GetName(instance);
}

void (*__PVE_HkdBreakableShape_SetName)(void * instance, void * name) __attribute__((ms_abi));
void HkdBreakableShape_SetName(void * instance, void * name) {
	printf("invoke HkdBreakableShape_SetName\n");
	return __PVE_HkdBreakableShape_SetName(instance, name);
}

float (*__PVE_HkdBreakableShape_GetVolume)(void * instance) __attribute__((ms_abi));
float HkdBreakableShape_GetVolume(void * instance) {
	printf("invoke HkdBreakableShape_GetVolume\n");
	return __PVE_HkdBreakableShape_GetVolume(instance);
}

void (*__PVE_HkdBreakableShape_SetVolume)(void * instance, float volume) __attribute__((ms_abi));
void HkdBreakableShape_SetVolume(void * instance, float volume) {
	printf("invoke HkdBreakableShape_SetVolume\n");
	return __PVE_HkdBreakableShape_SetVolume(instance, volume);
}

int (*__PVE_HkdBreakableShape_GetUserObject)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_GetUserObject(void * instance) {
	printf("invoke HkdBreakableShape_GetUserObject\n");
	return __PVE_HkdBreakableShape_GetUserObject(instance);
}

void (*__PVE_HkdBreakableShape_SetUserObject)(void * instance, int userObject) __attribute__((ms_abi));
void HkdBreakableShape_SetUserObject(void * instance, int userObject) {
	printf("invoke HkdBreakableShape_SetUserObject\n");
	return __PVE_HkdBreakableShape_SetUserObject(instance, userObject);
}

struct Vector3 (*__PVE_HkdBreakableShape_GetCoM)(void * instance) __attribute__((ms_abi));
struct Vector3 HkdBreakableShape_GetCoM(void * instance) {
	printf("invoke HkdBreakableShape_GetCoM\n");
	return __PVE_HkdBreakableShape_GetCoM(instance);
}

int (*__PVE_HkdBreakableShape_GetReferenceCount)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_GetReferenceCount(void * instance) {
	printf("invoke HkdBreakableShape_GetReferenceCount\n");
	return __PVE_HkdBreakableShape_GetReferenceCount(instance);
}

void (*__PVE_HkdBreakableShape_SetReferenceCount)(void * instance, int referenceCount) __attribute__((ms_abi));
void HkdBreakableShape_SetReferenceCount(void * instance, int referenceCount) {
	printf("invoke HkdBreakableShape_SetReferenceCount\n");
	return __PVE_HkdBreakableShape_SetReferenceCount(instance, referenceCount);
}

void (*__PVE_HkdBreakableShape_CopyData)(void * src, void * dst) __attribute__((ms_abi));
void HkdBreakableShape_CopyData(void * src, void * dst) {
	printf("invoke HkdBreakableShape_CopyData\n");
	return __PVE_HkdBreakableShape_CopyData(src, dst);
}

void (*__PVE_HkdBreakableShape_DisposeSharedMaterial)() __attribute__((ms_abi));
void HkdBreakableShape_DisposeSharedMaterial() {
	printf("invoke HkdBreakableShape_DisposeSharedMaterial\n");
	return __PVE_HkdBreakableShape_DisposeSharedMaterial();
}

void (*__PVE_HkdBreakableShape_AddConnection)(void * instance, void * connection) __attribute__((ms_abi));
void HkdBreakableShape_AddConnection(void * instance, void * connection) {
	printf("invoke HkdBreakableShape_AddConnection\n");
	return __PVE_HkdBreakableShape_AddConnection(instance, connection);
}

void (*__PVE_HkdBreakableShape_AddReference)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_AddReference(void * instance) {
	printf("invoke HkdBreakableShape_AddReference\n");
	return __PVE_HkdBreakableShape_AddReference(instance);
}

void (*__PVE_HkdBreakableShape_AddShape)(void * instance, void * shapeInfo) __attribute__((ms_abi));
void HkdBreakableShape_AddShape(void * instance, void * shapeInfo) {
	printf("invoke HkdBreakableShape_AddShape\n");
	return __PVE_HkdBreakableShape_AddShape(instance, shapeInfo);
}

void (*__PVE_HkdBreakableShape_AutoConnect)(void * instance, void * world) __attribute__((ms_abi));
void HkdBreakableShape_AutoConnect(void * instance, void * world) {
	printf("invoke HkdBreakableShape_AutoConnect\n");
	return __PVE_HkdBreakableShape_AutoConnect(instance, world);
}

struct HkMassProperties (*__PVE_HkdBreakableShape_BuildMassProperties)(void * instance) __attribute__((ms_abi));
struct HkMassProperties HkdBreakableShape_BuildMassProperties(void * instance) {
	printf("invoke HkdBreakableShape_BuildMassProperties\n");
	return __PVE_HkdBreakableShape_BuildMassProperties(instance);
}

float (*__PVE_HkdBreakableShape_CalculateGeometryVolume)(void * instance) __attribute__((ms_abi));
float HkdBreakableShape_CalculateGeometryVolume(void * instance) {
	printf("invoke HkdBreakableShape_CalculateGeometryVolume\n");
	return __PVE_HkdBreakableShape_CalculateGeometryVolume(instance);
}

void (*__PVE_HkdBreakableShape_ClearActions)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_ClearActions(void * instance) {
	printf("invoke HkdBreakableShape_ClearActions\n");
	return __PVE_HkdBreakableShape_ClearActions(instance);
}

void (*__PVE_HkdBreakableShape_ClearConnections)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_ClearConnections(void * instance) {
	printf("invoke HkdBreakableShape_ClearConnections\n");
	return __PVE_HkdBreakableShape_ClearConnections(instance);
}

void (*__PVE_HkdBreakableShape_ClearConnectionsRecursive)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_ClearConnectionsRecursive(void * instance) {
	printf("invoke HkdBreakableShape_ClearConnectionsRecursive\n");
	return __PVE_HkdBreakableShape_ClearConnectionsRecursive(instance);
}

void (*__PVE_HkdBreakableShape_ClearHandle)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_ClearHandle(void * instance) {
	printf("invoke HkdBreakableShape_ClearHandle\n");
	return __PVE_HkdBreakableShape_ClearHandle(instance);
}

void * (*__PVE_HkdBreakableShape_Clone)(void * instance) __attribute__((ms_abi));
void * HkdBreakableShape_Clone(void * instance) {
	printf("invoke HkdBreakableShape_Clone\n");
	return __PVE_HkdBreakableShape_Clone(instance);
}

void (*__PVE_HkdBreakableShape_ConnectSemiAccurate)(void * instance, void * world) __attribute__((ms_abi));
void HkdBreakableShape_ConnectSemiAccurate(void * instance, void * world) {
	printf("invoke HkdBreakableShape_ConnectSemiAccurate\n");
	return __PVE_HkdBreakableShape_ConnectSemiAccurate(instance, world);
}

void (*__PVE_HkdBreakableShape_DisableRefCount)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_DisableRefCount(void * instance) {
	printf("invoke HkdBreakableShape_DisableRefCount\n");
	return __PVE_HkdBreakableShape_DisableRefCount(instance);
}

void (*__PVE_HkdBreakableShape_DisableRefCountRecursively)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_DisableRefCountRecursively(void * instance) {
	printf("invoke HkdBreakableShape_DisableRefCountRecursively\n");
	return __PVE_HkdBreakableShape_DisableRefCountRecursively(instance);
}

void * (*__PVE_HkdBreakableShape_GetChild)(void * instance, int i) __attribute__((ms_abi));
void * HkdBreakableShape_GetChild(void * instance, int i) {
	printf("invoke HkdBreakableShape_GetChild\n");
	return __PVE_HkdBreakableShape_GetChild(instance, i);
}

void (*__PVE_HkdBreakableShape_GetChildren)(void * instance, void * returnShapeInstanceInfo) __attribute__((ms_abi));
void HkdBreakableShape_GetChildren(void * instance, void * returnShapeInstanceInfo) {
	printf("invoke HkdBreakableShape_GetChildren\n");
	return __PVE_HkdBreakableShape_GetChildren(instance, _PVE_Trampoline_Havok_HkdBreakableShape_ReturnShapeInstanceInfo(returnShapeInstanceInfo));
}

int (*__PVE_HkdBreakableShape_GetChildrenCount)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_GetChildrenCount(void * instance) {
	printf("invoke HkdBreakableShape_GetChildrenCount\n");
	return __PVE_HkdBreakableShape_GetChildrenCount(instance);
}

void * (*__PVE_HkdBreakableShape_GetChildShape)(void * instance, int i) __attribute__((ms_abi));
void * HkdBreakableShape_GetChildShape(void * instance, int i) {
	printf("invoke HkdBreakableShape_GetChildShape\n");
	return __PVE_HkdBreakableShape_GetChildShape(instance, i);
}

void (*__PVE_HkdBreakableShape_GetConnectionList)(void * instance, void * returnConnection) __attribute__((ms_abi));
void HkdBreakableShape_GetConnectionList(void * instance, void * returnConnection) {
	printf("invoke HkdBreakableShape_GetConnectionList\n");
	return __PVE_HkdBreakableShape_GetConnectionList(instance, _PVE_Trampoline_Havok_HkdBreakableShape_ReturnConnection(returnConnection));
}

float (*__PVE_HkdBreakableShape_GetMass)(void * instance) __attribute__((ms_abi));
float HkdBreakableShape_GetMass(void * instance) {
	printf("invoke HkdBreakableShape_GetMass\n");
	return __PVE_HkdBreakableShape_GetMass(instance);
}

void * (*__PVE_HkdBreakableShape_GetParent)(void * instance) __attribute__((ms_abi));
void * HkdBreakableShape_GetParent(void * instance) {
	printf("invoke HkdBreakableShape_GetParent\n");
	return __PVE_HkdBreakableShape_GetParent(instance);
}

void * (*__PVE_HkdBreakableShape_GetProperty)(void * instance, int key) __attribute__((ms_abi));
void * HkdBreakableShape_GetProperty(void * instance, int key) {
	printf("invoke HkdBreakableShape_GetProperty\n");
	return __PVE_HkdBreakableShape_GetProperty(instance, key);
}

void * (*__PVE_HkdBreakableShape_GetShape)(void * instance) __attribute__((ms_abi));
void * HkdBreakableShape_GetShape(void * instance) {
	printf("invoke HkdBreakableShape_GetShape\n");
	return __PVE_HkdBreakableShape_GetShape(instance);
}

float (*__PVE_HkdBreakableShape_GetStrenght)(void * instance) __attribute__((ms_abi));
float HkdBreakableShape_GetStrenght(void * instance) {
	printf("invoke HkdBreakableShape_GetStrenght\n");
	return __PVE_HkdBreakableShape_GetStrenght(instance);
}

int (*__PVE_HkdBreakableShape_GetTotalChildrenCount)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_GetTotalChildrenCount(void * instance) {
	printf("invoke HkdBreakableShape_GetTotalChildrenCount\n");
	return __PVE_HkdBreakableShape_GetTotalChildrenCount(instance);
}

int (*__PVE_HkdBreakableShape_HasFixedChildren)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_HasFixedChildren(void * instance) {
	printf("invoke HkdBreakableShape_HasFixedChildren\n");
	return __PVE_HkdBreakableShape_HasFixedChildren(instance);
}

int (*__PVE_HkdBreakableShape_HasProperty)(void * instance, int key) __attribute__((ms_abi));
int HkdBreakableShape_HasProperty(void * instance, int key) {
	printf("invoke HkdBreakableShape_HasProperty\n");
	return __PVE_HkdBreakableShape_HasProperty(instance, key);
}

void (*__PVE_HkdBreakableShape_InitIntegrity)(void * instance, float position) __attribute__((ms_abi));
void HkdBreakableShape_InitIntegrity(void * instance, float position) {
	printf("invoke HkdBreakableShape_InitIntegrity\n");
	return __PVE_HkdBreakableShape_InitIntegrity(instance, position);
}

int (*__PVE_HkdBreakableShape_IsChildOf)(void * instance, void * child) __attribute__((ms_abi));
int HkdBreakableShape_IsChildOf(void * instance, void * child) {
	printf("invoke HkdBreakableShape_IsChildOf\n");
	return __PVE_HkdBreakableShape_IsChildOf(instance, child);
}

int (*__PVE_HkdBreakableShape_IsCompound)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_IsCompound(void * instance) {
	printf("invoke HkdBreakableShape_IsCompound\n");
	return __PVE_HkdBreakableShape_IsCompound(instance);
}

int (*__PVE_HkdBreakableShape_IsDescendantOf)(void * instance, void * predecessor) __attribute__((ms_abi));
int HkdBreakableShape_IsDescendantOf(void * instance, void * predecessor) {
	printf("invoke HkdBreakableShape_IsDescendantOf\n");
	return __PVE_HkdBreakableShape_IsDescendantOf(instance, predecessor);
}

int (*__PVE_HkdBreakableShape_IsFixed)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_IsFixed(void * instance) {
	printf("invoke HkdBreakableShape_IsFixed\n");
	return __PVE_HkdBreakableShape_IsFixed(instance);
}

int (*__PVE_HkdBreakableShape_IsFracturePiece)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_IsFracturePiece(void * instance) {
	printf("invoke HkdBreakableShape_IsFracturePiece\n");
	return __PVE_HkdBreakableShape_IsFracturePiece(instance);
}

int (*__PVE_HkdBreakableShape_IsValid)(void * instance) __attribute__((ms_abi));
int HkdBreakableShape_IsValid(void * instance) {
	printf("invoke HkdBreakableShape_IsValid\n");
	return __PVE_HkdBreakableShape_IsValid(instance);
}

void (*__PVE_HkdBreakableShape_RemoveChild)(void * instance, int index) __attribute__((ms_abi));
void HkdBreakableShape_RemoveChild(void * instance, int index) {
	printf("invoke HkdBreakableShape_RemoveChild\n");
	return __PVE_HkdBreakableShape_RemoveChild(instance, index);
}

int (*__PVE_HkdBreakableShape_RemoveChildByName)(void * instance, void * shapeName) __attribute__((ms_abi));
int HkdBreakableShape_RemoveChildByName(void * instance, void * shapeName) {
	printf("invoke HkdBreakableShape_RemoveChildByName\n");
	return __PVE_HkdBreakableShape_RemoveChildByName(instance, shapeName);
}

void (*__PVE_HkdBreakableShape_RemoveConnection)(void * instance, void * connection) __attribute__((ms_abi));
void HkdBreakableShape_RemoveConnection(void * instance, void * connection) {
	printf("invoke HkdBreakableShape_RemoveConnection\n");
	return __PVE_HkdBreakableShape_RemoveConnection(instance, connection);
}

void (*__PVE_HkdBreakableShape_RemoveReference)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_RemoveReference(void * instance) {
	printf("invoke HkdBreakableShape_RemoveReference\n");
	return __PVE_HkdBreakableShape_RemoveReference(instance);
}

void (*__PVE_HkdBreakableShape_ReplaceChildren)(void * instance, int childrenCount, void * children) __attribute__((ms_abi));
void HkdBreakableShape_ReplaceChildren(void * instance, int childrenCount, void * children) {
	printf("invoke HkdBreakableShape_ReplaceChildren\n");
	return __PVE_HkdBreakableShape_ReplaceChildren(instance, childrenCount, children);
}

void (*__PVE_HkdBreakableShape_ReplaceConnections)(void * instance, int count, void * connections) __attribute__((ms_abi));
void HkdBreakableShape_ReplaceConnections(void * instance, int count, void * connections) {
	printf("invoke HkdBreakableShape_ReplaceConnections\n");
	return __PVE_HkdBreakableShape_ReplaceConnections(instance, count, connections);
}

void (*__PVE_HkdBreakableShape_SetAsDebris)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_SetAsDebris(void * instance) {
	printf("invoke HkdBreakableShape_SetAsDebris\n");
	return __PVE_HkdBreakableShape_SetAsDebris(instance);
}

void (*__PVE_HkdBreakableShape_SetAsDebrisRecursive)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_SetAsDebrisRecursive(void * instance) {
	printf("invoke HkdBreakableShape_SetAsDebrisRecursive\n");
	return __PVE_HkdBreakableShape_SetAsDebrisRecursive(instance);
}

void (*__PVE_HkdBreakableShape_SetAsFixed)(void * instance) __attribute__((ms_abi));
void HkdBreakableShape_SetAsFixed(void * instance) {
	printf("invoke HkdBreakableShape_SetAsFixed\n");
	return __PVE_HkdBreakableShape_SetAsFixed(instance);
}

void (*__PVE_HkdBreakableShape_SetChildrenParent)(void * instance, void * parent) __attribute__((ms_abi));
void HkdBreakableShape_SetChildrenParent(void * instance, void * parent) {
	printf("invoke HkdBreakableShape_SetChildrenParent\n");
	return __PVE_HkdBreakableShape_SetChildrenParent(instance, parent);
}

void (*__PVE_HkdBreakableShape_SetFlagRecursively)(void * instance, int flag) __attribute__((ms_abi));
void HkdBreakableShape_SetFlagRecursively(void * instance, int flag) {
	printf("invoke HkdBreakableShape_SetFlagRecursively\n");
	return __PVE_HkdBreakableShape_SetFlagRecursively(instance, flag);
}

void (*__PVE_HkdBreakableShape_SetHasFixedChildren)(void * instance, int has) __attribute__((ms_abi));
void HkdBreakableShape_SetHasFixedChildren(void * instance, int has) {
	printf("invoke HkdBreakableShape_SetHasFixedChildren\n");
	return __PVE_HkdBreakableShape_SetHasFixedChildren(instance, has);
}

void (*__PVE_HkdBreakableShape_SetMass)(void * instance, float mass) __attribute__((ms_abi));
void HkdBreakableShape_SetMass(void * instance, float mass) {
	printf("invoke HkdBreakableShape_SetMass\n");
	return __PVE_HkdBreakableShape_SetMass(instance, mass);
}

void (*__PVE_HkdBreakableShape_SetMassProperties)(void * instance, struct HkMassProperties massProperties) __attribute__((ms_abi));
void HkdBreakableShape_SetMassProperties(void * instance, struct HkMassProperties massProperties) {
	printf("invoke HkdBreakableShape_SetMassProperties\n");
	return __PVE_HkdBreakableShape_SetMassProperties(instance, massProperties);
}

void (*__PVE_HkdBreakableShape_SetMassRecursively)(void * instance, float mass) __attribute__((ms_abi));
void HkdBreakableShape_SetMassRecursively(void * instance, float mass) {
	printf("invoke HkdBreakableShape_SetMassRecursively\n");
	return __PVE_HkdBreakableShape_SetMassRecursively(instance, mass);
}

void (*__PVE_HkdBreakableShape_SetMotionQualityRecursively)(void * instance, int type) __attribute__((ms_abi));
void HkdBreakableShape_SetMotionQualityRecursively(void * instance, int type) {
	printf("invoke HkdBreakableShape_SetMotionQualityRecursively\n");
	return __PVE_HkdBreakableShape_SetMotionQualityRecursively(instance, type);
}

void (*__PVE_HkdBreakableShape_SetProperty)(void * instance, int key, void * prop) __attribute__((ms_abi));
void HkdBreakableShape_SetProperty(void * instance, int key, void * prop) {
	printf("invoke HkdBreakableShape_SetProperty\n");
	return __PVE_HkdBreakableShape_SetProperty(instance, key, prop);
}

void (*__PVE_HkdBreakableShape_SetPropertyRecursively)(void * instance, int key, void * prop) __attribute__((ms_abi));
void HkdBreakableShape_SetPropertyRecursively(void * instance, int key, void * prop) {
	printf("invoke HkdBreakableShape_SetPropertyRecursively\n");
	return __PVE_HkdBreakableShape_SetPropertyRecursively(instance, key, prop);
}

void (*__PVE_HkdBreakableShape_SetStrenght)(void * instance, float treshold) __attribute__((ms_abi));
void HkdBreakableShape_SetStrenght(void * instance, float treshold) {
	printf("invoke HkdBreakableShape_SetStrenght\n");
	return __PVE_HkdBreakableShape_SetStrenght(instance, treshold);
}

void (*__PVE_HkdBreakableShape_SetStrenghtRecursively)(void * instance, float treshold, float relativeSubpieceStrenght) __attribute__((ms_abi));
void HkdBreakableShape_SetStrenghtRecursively(void * instance, float treshold, float relativeSubpieceStrenght) {
	printf("invoke HkdBreakableShape_SetStrenghtRecursively\n");
	return __PVE_HkdBreakableShape_SetStrenghtRecursively(instance, treshold, relativeSubpieceStrenght);
}

void * (*__PVE_HkdCompoundBreakableShape_Create)(void * oldParent, int childCount, void * childShapes) __attribute__((ms_abi));
void * HkdCompoundBreakableShape_Create(void * oldParent, int childCount, void * childShapes) {
	printf("invoke HkdCompoundBreakableShape_Create\n");
	return __PVE_HkdCompoundBreakableShape_Create(oldParent, childCount, childShapes);
}

void (*__PVE_HkdCompoundBreakableShape_DisableChild)(void * instance, void * child) __attribute__((ms_abi));
void HkdCompoundBreakableShape_DisableChild(void * instance, void * child) {
	printf("invoke HkdCompoundBreakableShape_DisableChild\n");
	return __PVE_HkdCompoundBreakableShape_DisableChild(instance, child);
}

void (*__PVE_HkdCompoundBreakableShape_RecalcMassPropsFromChildren)(void * instance) __attribute__((ms_abi));
void HkdCompoundBreakableShape_RecalcMassPropsFromChildren(void * instance) {
	printf("invoke HkdCompoundBreakableShape_RecalcMassPropsFromChildren\n");
	return __PVE_HkdCompoundBreakableShape_RecalcMassPropsFromChildren(instance);
}

void * (*__PVE_HkdConnection_Create)() __attribute__((ms_abi));
void * HkdConnection_Create() {
	printf("invoke HkdConnection_Create\n");
	return __PVE_HkdConnection_Create();
}

void * (*__PVE_HkdConnection_CreateWithParams)(void * shapeA, void * shapeB, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 normal, float area) __attribute__((ms_abi));
void * HkdConnection_CreateWithParams(void * shapeA, void * shapeB, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 normal, float area) {
	printf("invoke HkdConnection_CreateWithParams\n");
	return __PVE_HkdConnection_CreateWithParams(shapeA, shapeB, pivotA, pivotB, normal, area);
}

void * (*__PVE_HkdConnection_GetShapeB)(void * instance) __attribute__((ms_abi));
void * HkdConnection_GetShapeB(void * instance) {
	printf("invoke HkdConnection_GetShapeB\n");
	return __PVE_HkdConnection_GetShapeB(instance);
}

void (*__PVE_HkdConnection_SetShapeB)(void * instance, void * shape) __attribute__((ms_abi));
void HkdConnection_SetShapeB(void * instance, void * shape) {
	printf("invoke HkdConnection_SetShapeB\n");
	return __PVE_HkdConnection_SetShapeB(instance, shape);
}

void * (*__PVE_HkdConnection_GetShapeA)(void * instance) __attribute__((ms_abi));
void * HkdConnection_GetShapeA(void * instance) {
	printf("invoke HkdConnection_GetShapeA\n");
	return __PVE_HkdConnection_GetShapeA(instance);
}

void (*__PVE_HkdConnection_SetShapeA)(void * instance, void * shape) __attribute__((ms_abi));
void HkdConnection_SetShapeA(void * instance, void * shape) {
	printf("invoke HkdConnection_SetShapeA\n");
	return __PVE_HkdConnection_SetShapeA(instance, shape);
}

void * (*__PVE_HkdConnection_GetShapeBName)(void * instance) __attribute__((ms_abi));
void * HkdConnection_GetShapeBName(void * instance) {
	printf("invoke HkdConnection_GetShapeBName\n");
	return __PVE_HkdConnection_GetShapeBName(instance);
}

void * (*__PVE_HkdConnection_GetShapeAName)(void * instance) __attribute__((ms_abi));
void * HkdConnection_GetShapeAName(void * instance) {
	printf("invoke HkdConnection_GetShapeAName\n");
	return __PVE_HkdConnection_GetShapeAName(instance);
}

float (*__PVE_HkdConnection_GetContactArea)(void * instance) __attribute__((ms_abi));
float HkdConnection_GetContactArea(void * instance) {
	printf("invoke HkdConnection_GetContactArea\n");
	return __PVE_HkdConnection_GetContactArea(instance);
}

void (*__PVE_HkdConnection_SetContactArea)(void * instance, float contactArea) __attribute__((ms_abi));
void HkdConnection_SetContactArea(void * instance, float contactArea) {
	printf("invoke HkdConnection_SetContactArea\n");
	return __PVE_HkdConnection_SetContactArea(instance, contactArea);
}

struct Vector3 (*__PVE_HkdConnection_GetSeparatingNormal)(void * instance) __attribute__((ms_abi));
struct Vector3 HkdConnection_GetSeparatingNormal(void * instance) {
	printf("invoke HkdConnection_GetSeparatingNormal\n");
	return __PVE_HkdConnection_GetSeparatingNormal(instance);
}

void (*__PVE_HkdConnection_SetSeparatingNormal)(void * instance, struct Vector3 separatingNormal) __attribute__((ms_abi));
void HkdConnection_SetSeparatingNormal(void * instance, struct Vector3 separatingNormal) {
	printf("invoke HkdConnection_SetSeparatingNormal\n");
	return __PVE_HkdConnection_SetSeparatingNormal(instance, separatingNormal);
}

struct Vector3 (*__PVE_HkdConnection_GetPivotB)(void * instance) __attribute__((ms_abi));
struct Vector3 HkdConnection_GetPivotB(void * instance) {
	printf("invoke HkdConnection_GetPivotB\n");
	return __PVE_HkdConnection_GetPivotB(instance);
}

void (*__PVE_HkdConnection_SetPivotB)(void * instance, struct Vector3 pivotB) __attribute__((ms_abi));
void HkdConnection_SetPivotB(void * instance, struct Vector3 pivotB) {
	printf("invoke HkdConnection_SetPivotB\n");
	return __PVE_HkdConnection_SetPivotB(instance, pivotB);
}

struct Vector3 (*__PVE_HkdConnection_GetPivotA)(void * instance) __attribute__((ms_abi));
struct Vector3 HkdConnection_GetPivotA(void * instance) {
	printf("invoke HkdConnection_GetPivotA\n");
	return __PVE_HkdConnection_GetPivotA(instance);
}

void (*__PVE_HkdConnection_SetPivotA)(void * instance, struct Vector3 pivotA) __attribute__((ms_abi));
void HkdConnection_SetPivotA(void * instance, struct Vector3 pivotA) {
	printf("invoke HkdConnection_SetPivotA\n");
	return __PVE_HkdConnection_SetPivotA(instance, pivotA);
}

void (*__PVE_HkdConnection_AddToCommonParent)(void * instance) __attribute__((ms_abi));
void HkdConnection_AddToCommonParent(void * instance) {
	printf("invoke HkdConnection_AddToCommonParent\n");
	return __PVE_HkdConnection_AddToCommonParent(instance);
}

int (*__PVE_HkdConnection_IsValid)(void * instance) __attribute__((ms_abi));
int HkdConnection_IsValid(void * instance) {
	printf("invoke HkdConnection_IsValid\n");
	return __PVE_HkdConnection_IsValid(instance);
}

void (*__PVE_HkdConnection_RemoveReference)(void * instance) __attribute__((ms_abi));
void HkdConnection_RemoveReference(void * instance) {
	printf("invoke HkdConnection_RemoveReference\n");
	return __PVE_HkdConnection_RemoveReference(instance);
}

void * (*__PVE_HkdFixedConnectivity_Create)() __attribute__((ms_abi));
void * HkdFixedConnectivity_Create() {
	printf("invoke HkdFixedConnectivity_Create\n");
	return __PVE_HkdFixedConnectivity_Create();
}

void (*__PVE_HkdFixedConnectivity_AddConnection)(void * instance, void * c) __attribute__((ms_abi));
void HkdFixedConnectivity_AddConnection(void * instance, void * c) {
	printf("invoke HkdFixedConnectivity_AddConnection\n");
	return __PVE_HkdFixedConnectivity_AddConnection(instance, c);
}

void (*__PVE_HkdFixedConnectivity_RemoveReference)(void * instance) __attribute__((ms_abi));
void HkdFixedConnectivity_RemoveReference(void * instance) {
	printf("invoke HkdFixedConnectivity_RemoveReference\n");
	return __PVE_HkdFixedConnectivity_RemoveReference(instance);
}

void * (*__PVE_HkdFixedConnectivity_CreateConnection)(struct Vector3 pivot, struct Vector3 separatingNormal, float contactArea, void * shape, void * targetBody, int shapeKey) __attribute__((ms_abi));
void * HkdFixedConnectivity_CreateConnection(struct Vector3 pivot, struct Vector3 separatingNormal, float contactArea, void * shape, void * targetBody, int shapeKey) {
	printf("invoke HkdFixedConnectivity_CreateConnection\n");
	return __PVE_HkdFixedConnectivity_CreateConnection(pivot, separatingNormal, contactArea, shape, targetBody, shapeKey);
}

void * (*__PVE_HkdFractureImpactDetails_Create)() __attribute__((ms_abi));
void * HkdFractureImpactDetails_Create() {
	printf("invoke HkdFractureImpactDetails_Create\n");
	return __PVE_HkdFractureImpactDetails_Create();
}

int (*__PVE_HkdFractureImpactDetails_GetFlag)(void * instance) __attribute__((ms_abi));
int HkdFractureImpactDetails_GetFlag(void * instance) {
	printf("invoke HkdFractureImpactDetails_GetFlag\n");
	return __PVE_HkdFractureImpactDetails_GetFlag(instance);
}

void (*__PVE_HkdFractureImpactDetails_SetFlag)(void * instance, int flag) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetFlag(void * instance, int flag) {
	printf("invoke HkdFractureImpactDetails_SetFlag\n");
	return __PVE_HkdFractureImpactDetails_SetFlag(instance, flag);
}

void * (*__PVE_HkdFractureImpactDetails_GetBreakingBody)(void * instance) __attribute__((ms_abi));
void * HkdFractureImpactDetails_GetBreakingBody(void * instance) {
	printf("invoke HkdFractureImpactDetails_GetBreakingBody\n");
	return __PVE_HkdFractureImpactDetails_GetBreakingBody(instance);
}

struct Vector3 (*__PVE_HkdFractureImpactDetails_GetContactPoint)(void * instance) __attribute__((ms_abi));
struct Vector3 HkdFractureImpactDetails_GetContactPoint(void * instance) {
	printf("invoke HkdFractureImpactDetails_GetContactPoint\n");
	return __PVE_HkdFractureImpactDetails_GetContactPoint(instance);
}

int (*__PVE_HkdFractureImpactDetails_IsValid)(void * instance) __attribute__((ms_abi));
int HkdFractureImpactDetails_IsValid(void * instance) {
	printf("invoke HkdFractureImpactDetails_IsValid\n");
	return __PVE_HkdFractureImpactDetails_IsValid(instance);
}

void (*__PVE_HkdFractureImpactDetails_RemoveReference)(void * instance) __attribute__((ms_abi));
void HkdFractureImpactDetails_RemoveReference(void * instance) {
	printf("invoke HkdFractureImpactDetails_RemoveReference\n");
	return __PVE_HkdFractureImpactDetails_RemoveReference(instance);
}

void (*__PVE_HkdFractureImpactDetails_SetBreakingBody)(void * instance, void * body) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetBreakingBody(void * instance, void * body) {
	printf("invoke HkdFractureImpactDetails_SetBreakingBody\n");
	return __PVE_HkdFractureImpactDetails_SetBreakingBody(instance, body);
}

void (*__PVE_HkdFractureImpactDetails_SetBreakingImpulse)(void * instance, float v) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetBreakingImpulse(void * instance, float v) {
	printf("invoke HkdFractureImpactDetails_SetBreakingImpulse\n");
	return __PVE_HkdFractureImpactDetails_SetBreakingImpulse(instance, v);
}

void (*__PVE_HkdFractureImpactDetails_SetContactPoint)(void * instance, struct Vector3 point) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetContactPoint(void * instance, struct Vector3 point) {
	printf("invoke HkdFractureImpactDetails_SetContactPoint\n");
	return __PVE_HkdFractureImpactDetails_SetContactPoint(instance, point);
}

void (*__PVE_HkdFractureImpactDetails_SetDestructionRadius)(void * instance, float v) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetDestructionRadius(void * instance, float v) {
	printf("invoke HkdFractureImpactDetails_SetDestructionRadius\n");
	return __PVE_HkdFractureImpactDetails_SetDestructionRadius(instance, v);
}

void (*__PVE_HkdFractureImpactDetails_SetOtherBody)(void * instance, void * otherBody) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetOtherBody(void * instance, void * otherBody) {
	printf("invoke HkdFractureImpactDetails_SetOtherBody\n");
	return __PVE_HkdFractureImpactDetails_SetOtherBody(instance, otherBody);
}

void (*__PVE_HkdFractureImpactDetails_SetParticleExpandVelocity)(void * instance, float v) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetParticleExpandVelocity(void * instance, float v) {
	printf("invoke HkdFractureImpactDetails_SetParticleExpandVelocity\n");
	return __PVE_HkdFractureImpactDetails_SetParticleExpandVelocity(instance, v);
}

void (*__PVE_HkdFractureImpactDetails_SetParticleMass)(void * instance, float mass) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetParticleMass(void * instance, float mass) {
	printf("invoke HkdFractureImpactDetails_SetParticleMass\n");
	return __PVE_HkdFractureImpactDetails_SetParticleMass(instance, mass);
}

void (*__PVE_HkdFractureImpactDetails_SetParticlePosition)(void * instance, struct Vector3 pos) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetParticlePosition(void * instance, struct Vector3 pos) {
	printf("invoke HkdFractureImpactDetails_SetParticlePosition\n");
	return __PVE_HkdFractureImpactDetails_SetParticlePosition(instance, pos);
}

void (*__PVE_HkdFractureImpactDetails_SetParticleVelocity)(void * instance, struct Vector3 vel) __attribute__((ms_abi));
void HkdFractureImpactDetails_SetParticleVelocity(void * instance, struct Vector3 vel) {
	printf("invoke HkdFractureImpactDetails_SetParticleVelocity\n");
	return __PVE_HkdFractureImpactDetails_SetParticleVelocity(instance, vel);
}

void (*__PVE_HkdFractureImpactDetails_ZeroCollidingParticleVelocity)(void * instance) __attribute__((ms_abi));
void HkdFractureImpactDetails_ZeroCollidingParticleVelocity(void * instance) {
	printf("invoke HkdFractureImpactDetails_ZeroCollidingParticleVelocity\n");
	return __PVE_HkdFractureImpactDetails_ZeroCollidingParticleVelocity(instance);
}

void * (*__PVE_HkdReplaceBodyEvent_GetOldBody)(void * instance) __attribute__((ms_abi));
void * HkdReplaceBodyEvent_GetOldBody(void * instance) {
	printf("invoke HkdReplaceBodyEvent_GetOldBody\n");
	return __PVE_HkdReplaceBodyEvent_GetOldBody(instance);
}

void (*__PVE_HkdReplaceBodyEvent_GetNewBodies)(void * instance, void * returnBreakableBodyInfo) __attribute__((ms_abi));
void HkdReplaceBodyEvent_GetNewBodies(void * instance, void * returnBreakableBodyInfo) {
	printf("invoke HkdReplaceBodyEvent_GetNewBodies\n");
	return __PVE_HkdReplaceBodyEvent_GetNewBodies(instance, _PVE_Trampoline_Havok_HkdReplaceBodyEvent_ReturnBreakableBodyInfo(returnBreakableBodyInfo));
}

void * (*__PVE_HkdShapeInstanceInfo_Create)(void * shape, struct Matrix transform) __attribute__((ms_abi));
void * HkdShapeInstanceInfo_Create(void * shape, struct Matrix transform) {
	printf("invoke HkdShapeInstanceInfo_Create\n");
	return __PVE_HkdShapeInstanceInfo_Create(shape, transform);
}

void * (*__PVE_HkdShapeInstanceInfo_CreateWithTranslation)(void * shape, struct Quaternion rotation, struct Vector3 translation) __attribute__((ms_abi));
void * HkdShapeInstanceInfo_CreateWithTranslation(void * shape, struct Quaternion rotation, struct Vector3 translation) {
	printf("invoke HkdShapeInstanceInfo_CreateWithTranslation\n");
	return __PVE_HkdShapeInstanceInfo_CreateWithTranslation(shape, rotation, translation);
}

void (*__PVE_HkdShapeInstanceInfo_Release)(void * instance) __attribute__((ms_abi));
void HkdShapeInstanceInfo_Release(void * instance) {
	printf("invoke HkdShapeInstanceInfo_Release\n");
	return __PVE_HkdShapeInstanceInfo_Release(instance);
}

short (*__PVE_HkdShapeInstanceInfo_GetDynamicParent)(void * instance) __attribute__((ms_abi));
short HkdShapeInstanceInfo_GetDynamicParent(void * instance) {
	printf("invoke HkdShapeInstanceInfo_GetDynamicParent\n");
	return __PVE_HkdShapeInstanceInfo_GetDynamicParent(instance);
}

void (*__PVE_HkdShapeInstanceInfo_SetDynamicParent)(void * instance, short dynamicParent) __attribute__((ms_abi));
void HkdShapeInstanceInfo_SetDynamicParent(void * instance, short dynamicParent) {
	printf("invoke HkdShapeInstanceInfo_SetDynamicParent\n");
	return __PVE_HkdShapeInstanceInfo_SetDynamicParent(instance, dynamicParent);
}

void * (*__PVE_HkdShapeInstanceInfo_GetShape)(void * instance) __attribute__((ms_abi));
void * HkdShapeInstanceInfo_GetShape(void * instance) {
	printf("invoke HkdShapeInstanceInfo_GetShape\n");
	return __PVE_HkdShapeInstanceInfo_GetShape(instance);
}

void * (*__PVE_HkdShapeInstanceInfo_GetShapeName)(void * instance) __attribute__((ms_abi));
void * HkdShapeInstanceInfo_GetShapeName(void * instance) {
	printf("invoke HkdShapeInstanceInfo_GetShapeName\n");
	return __PVE_HkdShapeInstanceInfo_GetShapeName(instance);
}

struct Vector3 (*__PVE_HkdShapeInstanceInfo_GetCoM)(void * instance) __attribute__((ms_abi));
struct Vector3 HkdShapeInstanceInfo_GetCoM(void * instance) {
	printf("invoke HkdShapeInstanceInfo_GetCoM\n");
	return __PVE_HkdShapeInstanceInfo_GetCoM(instance);
}

void (*__PVE_HkdShapeInstanceInfo_GetChildren)(void * instance, void * returnShapeInstanceInfo) __attribute__((ms_abi));
void HkdShapeInstanceInfo_GetChildren(void * instance, void * returnShapeInstanceInfo) {
	printf("invoke HkdShapeInstanceInfo_GetChildren\n");
	return __PVE_HkdShapeInstanceInfo_GetChildren(instance, _PVE_Trampoline_Havok_HkdShapeInstanceInfo_ReturnShapeInstanceInfo(returnShapeInstanceInfo));
}

struct Matrix (*__PVE_HkdShapeInstanceInfo_GetTransform)(void * instance) __attribute__((ms_abi));
struct Matrix HkdShapeInstanceInfo_GetTransform(void * instance) {
	printf("invoke HkdShapeInstanceInfo_GetTransform\n");
	return __PVE_HkdShapeInstanceInfo_GetTransform(instance);
}

int (*__PVE_HkdShapeInstanceInfo_InstanceOf)(void * instance, void * shape) __attribute__((ms_abi));
int HkdShapeInstanceInfo_InstanceOf(void * instance, void * shape) {
	printf("invoke HkdShapeInstanceInfo_InstanceOf\n");
	return __PVE_HkdShapeInstanceInfo_InstanceOf(instance, shape);
}

int (*__PVE_HkdShapeInstanceInfo_IsFracturePiece)(void * instance) __attribute__((ms_abi));
int HkdShapeInstanceInfo_IsFracturePiece(void * instance) {
	printf("invoke HkdShapeInstanceInfo_IsFracturePiece\n");
	return __PVE_HkdShapeInstanceInfo_IsFracturePiece(instance);
}

int (*__PVE_HkdShapeInstanceInfo_IsReferenceValid)(void * instance) __attribute__((ms_abi));
int HkdShapeInstanceInfo_IsReferenceValid(void * instance) {
	printf("invoke HkdShapeInstanceInfo_IsReferenceValid\n");
	return __PVE_HkdShapeInstanceInfo_IsReferenceValid(instance);
}

int (*__PVE_HkdShapeInstanceInfo_IsValid)(void * instance) __attribute__((ms_abi));
int HkdShapeInstanceInfo_IsValid(void * instance) {
	printf("invoke HkdShapeInstanceInfo_IsValid\n");
	return __PVE_HkdShapeInstanceInfo_IsValid(instance);
}

void (*__PVE_HkdShapeInstanceInfo_RemoveReference)(void * instance) __attribute__((ms_abi));
void HkdShapeInstanceInfo_RemoveReference(void * instance) {
	printf("invoke HkdShapeInstanceInfo_RemoveReference\n");
	return __PVE_HkdShapeInstanceInfo_RemoveReference(instance);
}

void (*__PVE_HkdShapeInstanceInfo_RemoveReferenceFromShape)(void * instance) __attribute__((ms_abi));
void HkdShapeInstanceInfo_RemoveReferenceFromShape(void * instance) {
	printf("invoke HkdShapeInstanceInfo_RemoveReferenceFromShape\n");
	return __PVE_HkdShapeInstanceInfo_RemoveReferenceFromShape(instance);
}

void (*__PVE_HkdShapeInstanceInfo_SetTransform)(void * instance, struct Matrix transform) __attribute__((ms_abi));
void HkdShapeInstanceInfo_SetTransform(void * instance, struct Matrix transform) {
	printf("invoke HkdShapeInstanceInfo_SetTransform\n");
	return __PVE_HkdShapeInstanceInfo_SetTransform(instance, transform);
}

void * (*__PVE_HkdWorld_Create)(void * world) __attribute__((ms_abi));
void * HkdWorld_Create(void * world) {
	printf("invoke HkdWorld_Create\n");
	return __PVE_HkdWorld_Create(world);
}

void (*__PVE_HkdWorld_AddBreakableBody)(void * instance, void * breakableBody) __attribute__((ms_abi));
void HkdWorld_AddBreakableBody(void * instance, void * breakableBody) {
	printf("invoke HkdWorld_AddBreakableBody\n");
	return __PVE_HkdWorld_AddBreakableBody(instance, breakableBody);
}

void (*__PVE_HkdWorld_RemoveBreakableBodyWithInfo)(void * instance, void * breakableBodyInfo) __attribute__((ms_abi));
void HkdWorld_RemoveBreakableBodyWithInfo(void * instance, void * breakableBodyInfo) {
	printf("invoke HkdWorld_RemoveBreakableBodyWithInfo\n");
	return __PVE_HkdWorld_RemoveBreakableBodyWithInfo(instance, breakableBodyInfo);
}

void (*__PVE_HkdWorld_RemoveBreakableBody)(void * instance, void * breakableBody) __attribute__((ms_abi));
void HkdWorld_RemoveBreakableBody(void * instance, void * breakableBody) {
	printf("invoke HkdWorld_RemoveBreakableBody\n");
	return __PVE_HkdWorld_RemoveBreakableBody(instance, breakableBody);
}

void (*__PVE_HkdWorld_TriggerDestruction)(void * instance, void * details) __attribute__((ms_abi));
void HkdWorld_TriggerDestruction(void * instance, void * details) {
	printf("invoke HkdWorld_TriggerDestruction\n");
	return __PVE_HkdWorld_TriggerDestruction(instance, details);
}

void (*__PVE_HkdWorld_Release)(void * instance) __attribute__((ms_abi));
void HkdWorld_Release(void * instance) {
	printf("invoke HkdWorld_Release\n");
	return __PVE_HkdWorld_Release(instance);
}

void * (*__PVE_HkDestructionStorage_Create)(void * world) __attribute__((ms_abi));
void * HkDestructionStorage_Create(void * world) {
	printf("invoke HkDestructionStorage_Create\n");
	return __PVE_HkDestructionStorage_Create(world);
}

void (*__PVE_HkDestructionStorage_CleanChildrenShapes)(void * instance, void * shape) __attribute__((ms_abi));
void HkDestructionStorage_CleanChildrenShapes(void * instance, void * shape) {
	printf("invoke HkDestructionStorage_CleanChildrenShapes\n");
	return __PVE_HkDestructionStorage_CleanChildrenShapes(instance, shape);
}

void * (*__PVE_HkDestructionStorage_CreateGeometry)(void * instance, void * meshShape, void * shapeName) __attribute__((ms_abi));
void * HkDestructionStorage_CreateGeometry(void * instance, void * meshShape, void * shapeName) {
	printf("invoke HkDestructionStorage_CreateGeometry\n");
	return __PVE_HkDestructionStorage_CreateGeometry(instance, meshShape, shapeName);
}

void * (*__PVE_HkDestructionStorage_MakeShapeFromData)(void * instance, int iCount, void * indices, int vCount, void * vPositions, void * vNormals, void * vTangents, void * vTexCoords, int sCount, void * sStarts, void * sTriCounts, void * sMatIdxes) __attribute__((ms_abi));
void * HkDestructionStorage_MakeShapeFromData(void * instance, int iCount, void * indices, int vCount, void * vPositions, void * vNormals, void * vTangents, void * vTexCoords, int sCount, void * sStarts, void * sTriCounts, void * sMatIdxes) {
	printf("invoke HkDestructionStorage_MakeShapeFromData\n");
	return __PVE_HkDestructionStorage_MakeShapeFromData(instance, iCount, indices, vCount, vPositions, vNormals, vTangents, vTexCoords, sCount, sStarts, sTriCounts, sMatIdxes);
}

void * (*__PVE_HkDestructionStorage_DumpDestructionData)(void * instance, int bSize, void * buffer) __attribute__((ms_abi));
void * HkDestructionStorage_DumpDestructionData(void * instance, int bSize, void * buffer) {
	printf("invoke HkDestructionStorage_DumpDestructionData\n");
	return __PVE_HkDestructionStorage_DumpDestructionData(instance, bSize, buffer);
}

void (*__PVE_HkDestructionStorage_FractureShape)(void * instance, void * shape, void * fracture) __attribute__((ms_abi));
void HkDestructionStorage_FractureShape(void * instance, void * shape, void * fracture) {
	printf("invoke HkDestructionStorage_FractureShape\n");
	return __PVE_HkDestructionStorage_FractureShape(instance, shape, fracture);
}

int (*__PVE_HkDestructionStorage_GetDataFromShape)(void * instance, void * shape, void * returnSectionData, void * returnIndex, void * returnVertex) __attribute__((ms_abi));
int HkDestructionStorage_GetDataFromShape(void * instance, void * shape, void * returnSectionData, void * returnIndex, void * returnVertex) {
	printf("invoke HkDestructionStorage_GetDataFromShape\n");
	return __PVE_HkDestructionStorage_GetDataFromShape(instance, shape, _PVE_Trampoline_Havok_HkDestructionStorage_ReturnSectionData(returnSectionData), _PVE_Trampoline_Havok_HkDestructionStorage_ReturnIndex(returnIndex), _PVE_Trampoline_Havok_HkDestructionStorage_ReturnVertex(returnVertex));
}

void (*__PVE_HkDestructionStorage_GetMaterialsOnRegisteredShapes)(void * instance, void * returnString) __attribute__((ms_abi));
void HkDestructionStorage_GetMaterialsOnRegisteredShapes(void * instance, void * returnString) {
	printf("invoke HkDestructionStorage_GetMaterialsOnRegisteredShapes\n");
	return __PVE_HkDestructionStorage_GetMaterialsOnRegisteredShapes(instance, _PVE_Trampoline_Havok_HkDestructionStorage_ReturnString(returnString));
}

void (*__PVE_HkDestructionStorage_GetRegisteredMaterials)(void * instance, void * returnString) __attribute__((ms_abi));
void HkDestructionStorage_GetRegisteredMaterials(void * instance, void * returnString) {
	printf("invoke HkDestructionStorage_GetRegisteredMaterials\n");
	return __PVE_HkDestructionStorage_GetRegisteredMaterials(instance, _PVE_Trampoline_Havok_HkDestructionStorage_ReturnString(returnString));
}

void (*__PVE_HkDestructionStorage_GetRegisteredShapes)(void * instance, void * returnString) __attribute__((ms_abi));
void HkDestructionStorage_GetRegisteredShapes(void * instance, void * returnString) {
	printf("invoke HkDestructionStorage_GetRegisteredShapes\n");
	return __PVE_HkDestructionStorage_GetRegisteredShapes(instance, _PVE_Trampoline_Havok_HkDestructionStorage_ReturnString(returnString));
}

void (*__PVE_HkDestructionStorage_LoadDestructionDataFromBuffer)(void * instance, int bSize, void * buffer, void * returnBreakableShape) __attribute__((ms_abi));
void HkDestructionStorage_LoadDestructionDataFromBuffer(void * instance, int bSize, void * buffer, void * returnBreakableShape) {
	printf("invoke HkDestructionStorage_LoadDestructionDataFromBuffer\n");
	return __PVE_HkDestructionStorage_LoadDestructionDataFromBuffer(instance, bSize, buffer, _PVE_Trampoline_Havok_HkDestructionStorage_ReturnBreakableShape(returnBreakableShape));
}

void (*__PVE_HkDestructionStorage_RegisterShape)(void * instance, void * shape, void * shapeName) __attribute__((ms_abi));
void HkDestructionStorage_RegisterShape(void * instance, void * shape, void * shapeName) {
	printf("invoke HkDestructionStorage_RegisterShape\n");
	return __PVE_HkDestructionStorage_RegisterShape(instance, shape, shapeName);
}

void (*__PVE_HkDestructionStorage_RegisterShapeWithGraphics)(void * instance, void * mesh, void * shape, void * shapeName) __attribute__((ms_abi));
void HkDestructionStorage_RegisterShapeWithGraphics(void * instance, void * mesh, void * shape, void * shapeName) {
	printf("invoke HkDestructionStorage_RegisterShapeWithGraphics\n");
	return __PVE_HkDestructionStorage_RegisterShapeWithGraphics(instance, mesh, shape, shapeName);
}

void (*__PVE_HkDestructionStorage_SaveDestructionData)(void * instance, void * shape, void * file) __attribute__((ms_abi));
void HkDestructionStorage_SaveDestructionData(void * instance, void * shape, void * file) {
	printf("invoke HkDestructionStorage_SaveDestructionData\n");
	return __PVE_HkDestructionStorage_SaveDestructionData(instance, shape, file);
}

void (*__PVE_HkDestructionStorage_SerializeDestructionData)(void * instance, void * world, void * returnByteArray) __attribute__((ms_abi));
void HkDestructionStorage_SerializeDestructionData(void * instance, void * world, void * returnByteArray) {
	printf("invoke HkDestructionStorage_SerializeDestructionData\n");
	return __PVE_HkDestructionStorage_SerializeDestructionData(instance, world, _PVE_Trampoline_Havok_HkDestructionStorage_ReturnByteArray(returnByteArray));
}

void (*__PVE_HkDestructionStorage_ReleasePtr)(void * instance) __attribute__((ms_abi));
void HkDestructionStorage_ReleasePtr(void * instance) {
	printf("invoke HkDestructionStorage_ReleasePtr\n");
	return __PVE_HkDestructionStorage_ReleasePtr(instance);
}

void * (*__PVE_HkEasePenetrationAction_Create)(void * body, float duration) __attribute__((ms_abi));
void * HkEasePenetrationAction_Create(void * body, float duration) {
	printf("invoke HkEasePenetrationAction_Create\n");
	return __PVE_HkEasePenetrationAction_Create(body, duration);
}

float (*__PVE_HkEasePenetrationAction_GetInitialAdditionalAllowedPenetrationDepth)(void * instance) __attribute__((ms_abi));
float HkEasePenetrationAction_GetInitialAdditionalAllowedPenetrationDepth(void * instance) {
	printf("invoke HkEasePenetrationAction_GetInitialAdditionalAllowedPenetrationDepth\n");
	return __PVE_HkEasePenetrationAction_GetInitialAdditionalAllowedPenetrationDepth(instance);
}

void (*__PVE_HkEasePenetrationAction_SetInitialAdditionalAllowedPenetrationDepth)(void * instance, float initialAdditionalAllowedPenetrationDepth) __attribute__((ms_abi));
void HkEasePenetrationAction_SetInitialAdditionalAllowedPenetrationDepth(void * instance, float initialAdditionalAllowedPenetrationDepth) {
	printf("invoke HkEasePenetrationAction_SetInitialAdditionalAllowedPenetrationDepth\n");
	return __PVE_HkEasePenetrationAction_SetInitialAdditionalAllowedPenetrationDepth(instance, initialAdditionalAllowedPenetrationDepth);
}

float (*__PVE_HkEasePenetrationAction_GetInitialAllowedPenetrationDepthMultiplier)(void * instance) __attribute__((ms_abi));
float HkEasePenetrationAction_GetInitialAllowedPenetrationDepthMultiplier(void * instance) {
	printf("invoke HkEasePenetrationAction_GetInitialAllowedPenetrationDepthMultiplier\n");
	return __PVE_HkEasePenetrationAction_GetInitialAllowedPenetrationDepthMultiplier(instance);
}

void (*__PVE_HkEasePenetrationAction_SetInitialAllowedPenetrationDepthMultiplier)(void * instance, float initialAllowedPenetrationDepthMultiplier) __attribute__((ms_abi));
void HkEasePenetrationAction_SetInitialAllowedPenetrationDepthMultiplier(void * instance, float initialAllowedPenetrationDepthMultiplier) {
	printf("invoke HkEasePenetrationAction_SetInitialAllowedPenetrationDepthMultiplier\n");
	return __PVE_HkEasePenetrationAction_SetInitialAllowedPenetrationDepthMultiplier(instance, initialAllowedPenetrationDepthMultiplier);
}

void * (*__PVE_HkGeometry_Create)() __attribute__((ms_abi));
void * HkGeometry_Create() {
	printf("invoke HkGeometry_Create\n");
	return __PVE_HkGeometry_Create();
}

void * (*__PVE_HkGeometry_CreateWithParams)(int vCount, void * vertices, int iCount, void * indices, int mCount, void * materials) __attribute__((ms_abi));
void * HkGeometry_CreateWithParams(int vCount, void * vertices, int iCount, void * indices, int mCount, void * materials) {
	printf("invoke HkGeometry_CreateWithParams\n");
	return __PVE_HkGeometry_CreateWithParams(vCount, vertices, iCount, indices, mCount, materials);
}

void (*__PVE_HkGeometry_Destroy)(void * instance) __attribute__((ms_abi));
void HkGeometry_Destroy(void * instance) {
	printf("invoke HkGeometry_Destroy\n");
	return __PVE_HkGeometry_Destroy(instance);
}

int (*__PVE_HkGeometry_GetTriangleCount)(void * instance) __attribute__((ms_abi));
int HkGeometry_GetTriangleCount(void * instance) {
	printf("invoke HkGeometry_GetTriangleCount\n");
	return __PVE_HkGeometry_GetTriangleCount(instance);
}

int (*__PVE_HkGeometry_GetVertexCount)(void * instance) __attribute__((ms_abi));
int HkGeometry_GetVertexCount(void * instance) {
	printf("invoke HkGeometry_GetVertexCount\n");
	return __PVE_HkGeometry_GetVertexCount(instance);
}

void (*__PVE_HkGeometry_Append)(void * instance, void * geometry, struct Matrix matrix) __attribute__((ms_abi));
void HkGeometry_Append(void * instance, void * geometry, struct Matrix matrix) {
	printf("invoke HkGeometry_Append\n");
	return __PVE_HkGeometry_Append(instance, geometry, matrix);
}

void (*__PVE_HkGeometry_GetTriangle)(void * instance, int triangleIndex, void * outTriangle) __attribute__((ms_abi));
void HkGeometry_GetTriangle(void * instance, int triangleIndex, void * outTriangle) {
	printf("invoke HkGeometry_GetTriangle\n");
	return __PVE_HkGeometry_GetTriangle(instance, triangleIndex, outTriangle);
}

struct Vector3 (*__PVE_HkGeometry_GetVertex)(void * instance, int vertexIndex) __attribute__((ms_abi));
struct Vector3 HkGeometry_GetVertex(void * instance, int vertexIndex) {
	printf("invoke HkGeometry_GetVertex\n");
	return __PVE_HkGeometry_GetVertex(instance, vertexIndex);
}

void (*__PVE_HkGeometry_SetGeometry)(void * instance, int vCount, void * vertices, int iCount, void * indices, int mCount, void * materials) __attribute__((ms_abi));
void HkGeometry_SetGeometry(void * instance, int vCount, void * vertices, int iCount, void * indices, int mCount, void * materials) {
	printf("invoke HkGeometry_SetGeometry\n");
	return __PVE_HkGeometry_SetGeometry(instance, vCount, vertices, iCount, indices, mCount, materials);
}

int (*__PVE_HkGroupFilter_CalcFilterInfo)(int layer, int systemGroup, int subSystemId, int subSystemDontCollideWith) __attribute__((ms_abi));
int HkGroupFilter_CalcFilterInfo(int layer, int systemGroup, int subSystemId, int subSystemDontCollideWith) {
	printf("invoke HkGroupFilter_CalcFilterInfo\n");
	return __PVE_HkGroupFilter_CalcFilterInfo(layer, systemGroup, subSystemId, subSystemDontCollideWith);
}

int (*__PVE_HkGroupFilter_GetLayerFromFilterInfo)(int filterInfo) __attribute__((ms_abi));
int HkGroupFilter_GetLayerFromFilterInfo(int filterInfo) {
	printf("invoke HkGroupFilter_GetLayerFromFilterInfo\n");
	return __PVE_HkGroupFilter_GetLayerFromFilterInfo(filterInfo);
}

int (*__PVE_HkGroupFilter_getSubSystemDontCollideWithFromFilterInfo)(int filterInfo) __attribute__((ms_abi));
int HkGroupFilter_getSubSystemDontCollideWithFromFilterInfo(int filterInfo) {
	printf("invoke HkGroupFilter_getSubSystemDontCollideWithFromFilterInfo\n");
	return __PVE_HkGroupFilter_getSubSystemDontCollideWithFromFilterInfo(filterInfo);
}

int (*__PVE_HkGroupFilter_GetSubSystemIdFromFilterInfo)(int filterInfo) __attribute__((ms_abi));
int HkGroupFilter_GetSubSystemIdFromFilterInfo(int filterInfo) {
	printf("invoke HkGroupFilter_GetSubSystemIdFromFilterInfo\n");
	return __PVE_HkGroupFilter_GetSubSystemIdFromFilterInfo(filterInfo);
}

int (*__PVE_HkGroupFilter_GetSystemGroupFromFilterInfo)(int filterInfo) __attribute__((ms_abi));
int HkGroupFilter_GetSystemGroupFromFilterInfo(int filterInfo) {
	printf("invoke HkGroupFilter_GetSystemGroupFromFilterInfo\n");
	return __PVE_HkGroupFilter_GetSystemGroupFromFilterInfo(filterInfo);
}

int (*__PVE_HkGroupFilter_SetLayer)(int filterInfo, int newLayer) __attribute__((ms_abi));
int HkGroupFilter_SetLayer(int filterInfo, int newLayer) {
	printf("invoke HkGroupFilter_SetLayer\n");
	return __PVE_HkGroupFilter_SetLayer(filterInfo, newLayer);
}

void (*__PVE_HkGroupFilter_DisableCollisionsBetween)(void * instance, int layerA, int layerB) __attribute__((ms_abi));
void HkGroupFilter_DisableCollisionsBetween(void * instance, int layerA, int layerB) {
	printf("invoke HkGroupFilter_DisableCollisionsBetween\n");
	return __PVE_HkGroupFilter_DisableCollisionsBetween(instance, layerA, layerB);
}

void (*__PVE_HkGroupFilter_DisableCollisionsUsingBitfield)(void * instance, int layerBitsA, int layerBitsB) __attribute__((ms_abi));
void HkGroupFilter_DisableCollisionsUsingBitfield(void * instance, int layerBitsA, int layerBitsB) {
	printf("invoke HkGroupFilter_DisableCollisionsUsingBitfield\n");
	return __PVE_HkGroupFilter_DisableCollisionsUsingBitfield(instance, layerBitsA, layerBitsB);
}

void (*__PVE_HkGroupFilter_EnableCollisionsBetween)(void * instance, int layerA, int layerB) __attribute__((ms_abi));
void HkGroupFilter_EnableCollisionsBetween(void * instance, int layerA, int layerB) {
	printf("invoke HkGroupFilter_EnableCollisionsBetween\n");
	return __PVE_HkGroupFilter_EnableCollisionsBetween(instance, layerA, layerB);
}

void (*__PVE_HkGroupFilter_EnableCollisionsUsingBitfield)(void * instance, int layerBitsA, int layerBitsB) __attribute__((ms_abi));
void HkGroupFilter_EnableCollisionsUsingBitfield(void * instance, int layerBitsA, int layerBitsB) {
	printf("invoke HkGroupFilter_EnableCollisionsUsingBitfield\n");
	return __PVE_HkGroupFilter_EnableCollisionsUsingBitfield(instance, layerBitsA, layerBitsB);
}

int (*__PVE_HkGroupFilter_GetNewSystemGroup)(void * instance) __attribute__((ms_abi));
int HkGroupFilter_GetNewSystemGroup(void * instance) {
	printf("invoke HkGroupFilter_GetNewSystemGroup\n");
	return __PVE_HkGroupFilter_GetNewSystemGroup(instance);
}

void * (*__PVE_HkInertiaTensorComputer_Create)() __attribute__((ms_abi));
void * HkInertiaTensorComputer_Create() {
	printf("invoke HkInertiaTensorComputer_Create\n");
	return __PVE_HkInertiaTensorComputer_Create();
}

void (*__PVE_HkInertiaTensorComputer_CombineMassPropertiesInstance)(void * instance, void * massElements, int count, void * returnMassProperties) __attribute__((ms_abi));
void HkInertiaTensorComputer_CombineMassPropertiesInstance(void * instance, void * massElements, int count, void * returnMassProperties) {
	printf("invoke HkInertiaTensorComputer_CombineMassPropertiesInstance\n");
	return __PVE_HkInertiaTensorComputer_CombineMassPropertiesInstance(instance, massElements, count, returnMassProperties);
}

void (*__PVE_HkInertiaTensorComputer_Release)(void * instance) __attribute__((ms_abi));
void HkInertiaTensorComputer_Release(void * instance) {
	printf("invoke HkInertiaTensorComputer_Release\n");
	return __PVE_HkInertiaTensorComputer_Release(instance);
}

void (*__PVE_HkInertiaTensorComputer_ComputeBoxVolumeMassProperties)(struct Vector3 halfExtents, float mass, void * returnMassProperties) __attribute__((ms_abi));
void HkInertiaTensorComputer_ComputeBoxVolumeMassProperties(struct Vector3 halfExtents, float mass, void * returnMassProperties) {
	printf("invoke HkInertiaTensorComputer_ComputeBoxVolumeMassProperties\n");
	return __PVE_HkInertiaTensorComputer_ComputeBoxVolumeMassProperties(halfExtents, mass, returnMassProperties);
}

void (*__PVE_HkInertiaTensorComputer_ComputeCapsuleVolumeMassProperties)(struct Vector3 startAxis, struct Vector3 endAxis, float radius, float mass, void * returnMassProperties) __attribute__((ms_abi));
void HkInertiaTensorComputer_ComputeCapsuleVolumeMassProperties(struct Vector3 startAxis, struct Vector3 endAxis, float radius, float mass, void * returnMassProperties) {
	printf("invoke HkInertiaTensorComputer_ComputeCapsuleVolumeMassProperties\n");
	return __PVE_HkInertiaTensorComputer_ComputeCapsuleVolumeMassProperties(startAxis, endAxis, radius, mass, returnMassProperties);
}

void (*__PVE_HkInertiaTensorComputer_ComputeCylinderVolumeMassProperties)(struct Vector3 startAxis, struct Vector3 endAxis, float radius, float mass, void * returnMassProperties) __attribute__((ms_abi));
void HkInertiaTensorComputer_ComputeCylinderVolumeMassProperties(struct Vector3 startAxis, struct Vector3 endAxis, float radius, float mass, void * returnMassProperties) {
	printf("invoke HkInertiaTensorComputer_ComputeCylinderVolumeMassProperties\n");
	return __PVE_HkInertiaTensorComputer_ComputeCylinderVolumeMassProperties(startAxis, endAxis, radius, mass, returnMassProperties);
}

void (*__PVE_HkInertiaTensorComputer_ComputeSphereVolumeMassProperties)(float radius, float mass, void * returnMassProperties) __attribute__((ms_abi));
void HkInertiaTensorComputer_ComputeSphereVolumeMassProperties(float radius, float mass, void * returnMassProperties) {
	printf("invoke HkInertiaTensorComputer_ComputeSphereVolumeMassProperties\n");
	return __PVE_HkInertiaTensorComputer_ComputeSphereVolumeMassProperties(radius, mass, returnMassProperties);
}

void (*__PVE_HkMemorySnapshot_Diff)(void * a, void * b, void * inA, void * inB) __attribute__((ms_abi));
void HkMemorySnapshot_Diff(void * a, void * b, void * inA, void * inB) {
	printf("invoke HkMemorySnapshot_Diff\n");
	return __PVE_HkMemorySnapshot_Diff(a, b, inA, inB);
}

int (*__PVE_HkShapeCutterUtil_Cut)(void * shape, struct Vector4 plane, void * aabbMin, void * aabbMax) __attribute__((ms_abi));
int HkShapeCutterUtil_Cut(void * shape, struct Vector4 plane, void * aabbMin, void * aabbMax) {
	printf("invoke HkShapeCutterUtil_Cut\n");
	return __PVE_HkShapeCutterUtil_Cut(shape, plane, aabbMin, aabbMax);
}

void * (*__PVE_HkSimpleValueProperty_CreateFloat)(float value) __attribute__((ms_abi));
void * HkSimpleValueProperty_CreateFloat(float value) {
	printf("invoke HkSimpleValueProperty_CreateFloat\n");
	return __PVE_HkSimpleValueProperty_CreateFloat(value);
}

void * (*__PVE_HkSimpleValueProperty_CreateUInt)(int value) __attribute__((ms_abi));
void * HkSimpleValueProperty_CreateUInt(int value) {
	printf("invoke HkSimpleValueProperty_CreateUInt\n");
	return __PVE_HkSimpleValueProperty_CreateUInt(value);
}

void * (*__PVE_HkSimpleValueProperty_CreateInt)(int value) __attribute__((ms_abi));
void * HkSimpleValueProperty_CreateInt(int value) {
	printf("invoke HkSimpleValueProperty_CreateInt\n");
	return __PVE_HkSimpleValueProperty_CreateInt(value);
}

float (*__PVE_HkSimpleValueProperty_GetValueFloat)(void * instance) __attribute__((ms_abi));
float HkSimpleValueProperty_GetValueFloat(void * instance) {
	printf("invoke HkSimpleValueProperty_GetValueFloat\n");
	return __PVE_HkSimpleValueProperty_GetValueFloat(instance);
}

void (*__PVE_HkSimpleValueProperty_SetValueFloat)(void * instance, float valueFloat) __attribute__((ms_abi));
void HkSimpleValueProperty_SetValueFloat(void * instance, float valueFloat) {
	printf("invoke HkSimpleValueProperty_SetValueFloat\n");
	return __PVE_HkSimpleValueProperty_SetValueFloat(instance, valueFloat);
}

int (*__PVE_HkSimpleValueProperty_GetValueUInt)(void * instance) __attribute__((ms_abi));
int HkSimpleValueProperty_GetValueUInt(void * instance) {
	printf("invoke HkSimpleValueProperty_GetValueUInt\n");
	return __PVE_HkSimpleValueProperty_GetValueUInt(instance);
}

void (*__PVE_HkSimpleValueProperty_SetValueUInt)(void * instance, int valueUInt) __attribute__((ms_abi));
void HkSimpleValueProperty_SetValueUInt(void * instance, int valueUInt) {
	printf("invoke HkSimpleValueProperty_SetValueUInt\n");
	return __PVE_HkSimpleValueProperty_SetValueUInt(instance, valueUInt);
}

int (*__PVE_HkSimpleValueProperty_GetValueInt)(void * instance) __attribute__((ms_abi));
int HkSimpleValueProperty_GetValueInt(void * instance) {
	printf("invoke HkSimpleValueProperty_GetValueInt\n");
	return __PVE_HkSimpleValueProperty_GetValueInt(instance);
}

void (*__PVE_HkSimpleValueProperty_SetValueInt)(void * instance, int calueInt) __attribute__((ms_abi));
void HkSimpleValueProperty_SetValueInt(void * instance, int calueInt) {
	printf("invoke HkSimpleValueProperty_SetValueInt\n");
	return __PVE_HkSimpleValueProperty_SetValueInt(instance, calueInt);
}

void * (*__PVE_HkVec3IProperty_Create)(struct Vector3I value) __attribute__((ms_abi));
void * HkVec3IProperty_Create(struct Vector3I value) {
	printf("invoke HkVec3IProperty_Create\n");
	return __PVE_HkVec3IProperty_Create(value);
}

struct Vector3I (*__PVE_HkVec3IProperty_GetValue)(void * instance) __attribute__((ms_abi));
struct Vector3I HkVec3IProperty_GetValue(void * instance) {
	printf("invoke HkVec3IProperty_GetValue\n");
	return __PVE_HkVec3IProperty_GetValue(instance);
}

void (*__PVE_HkVec3IProperty_SetValue)(void * instance, struct Vector3I value) __attribute__((ms_abi));
void HkVec3IProperty_SetValue(void * instance, struct Vector3I value) {
	printf("invoke HkVec3IProperty_SetValue\n");
	return __PVE_HkVec3IProperty_SetValue(instance, value);
}

void * (*__PVE_HkWheelResponseModifierUtil_Create)(void * rigidBody, void * softness, void * acceleration) __attribute__((ms_abi));
void * HkWheelResponseModifierUtil_Create(void * rigidBody, void * softness, void * acceleration) {
	printf("invoke HkWheelResponseModifierUtil_Create\n");
	return __PVE_HkWheelResponseModifierUtil_Create(rigidBody, _PVE_Trampoline_Havok_HkWheelResponseModifierUtil_CalculateModifier(softness), _PVE_Trampoline_Havok_HkWheelResponseModifierUtil_CalculateModifier(acceleration));
}

void (*__PVE_HkWheelResponseModifierUtil_Release)(void * instance) __attribute__((ms_abi));
void HkWheelResponseModifierUtil_Release(void * instance) {
	printf("invoke HkWheelResponseModifierUtil_Release\n");
	return __PVE_HkWheelResponseModifierUtil_Release(instance);
}

void * (*__PVE_HkActivationListener_Create)(void * onActivate, void * onDeactivate) __attribute__((ms_abi));
void * HkActivationListener_Create(void * onActivate, void * onDeactivate) {
	printf("invoke HkActivationListener_Create\n");
	return __PVE_HkActivationListener_Create(_PVE_Trampoline_Havok_HkActivationListener_HkActivationHandlerCpp(onActivate), _PVE_Trampoline_Havok_HkActivationListener_HkActivationHandlerCpp(onDeactivate));
}

void (*__PVE_HkBaseSystem_Init)(int solverMemorySize, void * log, int deepProfiling) __attribute__((ms_abi));
void HkBaseSystem_Init(int solverMemorySize, void * log, int deepProfiling) {
	printf("invoke HkBaseSystem_Init\n");
	return __PVE_HkBaseSystem_Init(solverMemorySize, _PVE_Trampoline_Havok_HkBaseSystem_Log(log), deepProfiling);
}

void (*__PVE_HkBaseSystem_Quit)() __attribute__((ms_abi));
void HkBaseSystem_Quit() {
	printf("invoke HkBaseSystem_Quit\n");
	return __PVE_HkBaseSystem_Quit();
}

void * (*__PVE_HkBaseSystem_InitThread)() __attribute__((ms_abi));
void * HkBaseSystem_InitThread() {
	printf("invoke HkBaseSystem_InitThread\n");
	return __PVE_HkBaseSystem_InitThread();
}

void (*__PVE_HkBaseSystem_QuitThread)(void * threadRouter) __attribute__((ms_abi));
void HkBaseSystem_QuitThread(void * threadRouter) {
	printf("invoke HkBaseSystem_QuitThread\n");
	return __PVE_HkBaseSystem_QuitThread(threadRouter);
}

void (*__PVE_HkBaseSystem_GetVersionInfo)(void * buffer) __attribute__((ms_abi));
void HkBaseSystem_GetVersionInfo(void * buffer) {
	printf("invoke HkBaseSystem_GetVersionInfo\n");
	return __PVE_HkBaseSystem_GetVersionInfo(buffer);
}

void (*__PVE_HkBaseSystem_GetMemoryStatistics)(void * buffer) __attribute__((ms_abi));
void HkBaseSystem_GetMemoryStatistics(void * buffer) {
	printf("invoke HkBaseSystem_GetMemoryStatistics\n");
	return __PVE_HkBaseSystem_GetMemoryStatistics(buffer);
}

void (*__PVE_HkBaseSystem_EnableAssert)(int assertId, int enable) __attribute__((ms_abi));
void HkBaseSystem_EnableAssert(int assertId, int enable) {
	printf("invoke HkBaseSystem_EnableAssert\n");
	return __PVE_HkBaseSystem_EnableAssert(assertId, enable);
}

int (*__PVE_HkBaseSystem_IsEnabled)(int assertId) __attribute__((ms_abi));
int HkBaseSystem_IsEnabled(int assertId) {
	printf("invoke HkBaseSystem_IsEnabled\n");
	return __PVE_HkBaseSystem_IsEnabled(assertId);
}

int (*__PVE_HkBaseSystem_IsDestructionEnabled)() __attribute__((ms_abi));
int HkBaseSystem_IsDestructionEnabled() {
	printf("invoke HkBaseSystem_IsDestructionEnabled\n");
	return __PVE_HkBaseSystem_IsDestructionEnabled();
}

void (*__PVE_HkBaseSystem_OnSimulationFrameStarted)(long int frameNumber) __attribute__((ms_abi));
void HkBaseSystem_OnSimulationFrameStarted(long int frameNumber) {
	printf("invoke HkBaseSystem_OnSimulationFrameStarted\n");
	return __PVE_HkBaseSystem_OnSimulationFrameStarted(frameNumber);
}

void (*__PVE_HkBaseSystem_OnSimulationFrameFinished)() __attribute__((ms_abi));
void HkBaseSystem_OnSimulationFrameFinished() {
	printf("invoke HkBaseSystem_OnSimulationFrameFinished\n");
	return __PVE_HkBaseSystem_OnSimulationFrameFinished();
}

int (*__PVE_HkBaseSystem_GetKeyCodes)(void * keyCodes) __attribute__((ms_abi));
int HkBaseSystem_GetKeyCodes(void * keyCodes) {
	printf("invoke HkBaseSystem_GetKeyCodes\n");
	return __PVE_HkBaseSystem_GetKeyCodes(keyCodes);
}

int (*__PVE_HkBaseSystem_IsOutOfMemory)() __attribute__((ms_abi));
int HkBaseSystem_IsOutOfMemory() {
	printf("invoke HkBaseSystem_IsOutOfMemory\n");
	return __PVE_HkBaseSystem_IsOutOfMemory();
}

long int (*__PVE_HkBaseSystem_GetCurrentMemoryConsumption)() __attribute__((ms_abi));
long int HkBaseSystem_GetCurrentMemoryConsumption() {
	printf("invoke HkBaseSystem_GetCurrentMemoryConsumption\n");
	return __PVE_HkBaseSystem_GetCurrentMemoryConsumption();
}

int (*__PVE_HkCollisionEvent_GetSource)(void * instance) __attribute__((ms_abi));
int HkCollisionEvent_GetSource(void * instance) {
	printf("invoke HkCollisionEvent_GetSource\n");
	return __PVE_HkCollisionEvent_GetSource(instance);
}

void * (*__PVE_HkCollisionEvent_GetRigidBody)(void * instance, int bodyIndex) __attribute__((ms_abi));
void * HkCollisionEvent_GetRigidBody(void * instance, int bodyIndex) {
	printf("invoke HkCollisionEvent_GetRigidBody\n");
	return __PVE_HkCollisionEvent_GetRigidBody(instance, bodyIndex);
}

void * (*__PVE_HkCollisionEvent_GetBodyA)(void * instance) __attribute__((ms_abi));
void * HkCollisionEvent_GetBodyA(void * instance) {
	printf("invoke HkCollisionEvent_GetBodyA\n");
	return __PVE_HkCollisionEvent_GetBodyA(instance);
}

void * (*__PVE_HkCollisionEvent_GetBodyB)(void * instance) __attribute__((ms_abi));
void * HkCollisionEvent_GetBodyB(void * instance) {
	printf("invoke HkCollisionEvent_GetBodyB\n");
	return __PVE_HkCollisionEvent_GetBodyB(instance);
}

int (*__PVE_HkCollisionEvent_SetImpulse)(void * instance, float impulse) __attribute__((ms_abi));
int HkCollisionEvent_SetImpulse(void * instance, float impulse) {
	printf("invoke HkCollisionEvent_SetImpulse\n");
	return __PVE_HkCollisionEvent_SetImpulse(instance, impulse);
}

void (*__PVE_HkCollisionEvent_SetImpulseScaling)(void * instance, float impulse, float maxAccel) __attribute__((ms_abi));
void HkCollisionEvent_SetImpulseScaling(void * instance, float impulse, float maxAccel) {
	printf("invoke HkCollisionEvent_SetImpulseScaling\n");
	return __PVE_HkCollisionEvent_SetImpulseScaling(instance, impulse, maxAccel);
}

int (*__PVE_HkCollisionEvent_GetContactPointCount)(void * instance) __attribute__((ms_abi));
int HkCollisionEvent_GetContactPointCount(void * instance) {
	printf("invoke HkCollisionEvent_GetContactPointCount\n");
	return __PVE_HkCollisionEvent_GetContactPointCount(instance);
}

void (*__PVE_HkCollisionEvent_Disable)(void * instance) __attribute__((ms_abi));
void HkCollisionEvent_Disable(void * instance) {
	printf("invoke HkCollisionEvent_Disable\n");
	return __PVE_HkCollisionEvent_Disable(instance);
}

void * (*__PVE_HkCollisionEvent_GetContactPointPropertiesAt)(void * instance, int index) __attribute__((ms_abi));
void * HkCollisionEvent_GetContactPointPropertiesAt(void * instance, int index) {
	printf("invoke HkCollisionEvent_GetContactPointPropertiesAt\n");
	return __PVE_HkCollisionEvent_GetContactPointPropertiesAt(instance, index);
}

void (*__PVE_HkCollisionEvent_GetOffsets)(void * bodyPointerOffset) __attribute__((ms_abi));
void HkCollisionEvent_GetOffsets(void * bodyPointerOffset) {
	printf("invoke HkCollisionEvent_GetOffsets\n");
	return __PVE_HkCollisionEvent_GetOffsets(bodyPointerOffset);
}

void * (*__PVE_HkConstraintProjectorListener_Create)(void * world) __attribute__((ms_abi));
void * HkConstraintProjectorListener_Create(void * world) {
	printf("invoke HkConstraintProjectorListener_Create\n");
	return __PVE_HkConstraintProjectorListener_Create(world);
}

void (*__PVE_HkConstraintProjectorListener_Release)(void * listener) __attribute__((ms_abi));
void HkConstraintProjectorListener_Release(void * listener) {
	printf("invoke HkConstraintProjectorListener_Release\n");
	return __PVE_HkConstraintProjectorListener_Release(listener);
}

void * (*__PVE_HkContactListener_Create)(void * onContact, void * collisionAdded, void * collisionRemoved, int callbackLimit) __attribute__((ms_abi));
void * HkContactListener_Create(void * onContact, void * collisionAdded, void * collisionRemoved, int callbackLimit) {
	printf("invoke HkContactListener_Create\n");
	return __PVE_HkContactListener_Create(_PVE_Trampoline_Havok_HkContactListener_ContactPointHandler(onContact), _PVE_Trampoline_Havok_HkContactListener_CollisionHandler(collisionAdded), _PVE_Trampoline_Havok_HkContactListener_CollisionHandler(collisionRemoved), callbackLimit);
}

void (*__PVE_HkContactListener_SetCallbackLimit)(void * instance, int value) __attribute__((ms_abi));
void HkContactListener_SetCallbackLimit(void * instance, int value) {
	printf("invoke HkContactListener_SetCallbackLimit\n");
	return __PVE_HkContactListener_SetCallbackLimit(instance, value);
}

void (*__PVE_HkContactListener_ResetLimit)(void * instance) __attribute__((ms_abi));
void HkContactListener_ResetLimit(void * instance) {
	printf("invoke HkContactListener_ResetLimit\n");
	return __PVE_HkContactListener_ResetLimit(instance);
}

struct Vector3 (*__PVE_HkContactPoint_GetPosition)(void * instance) __attribute__((ms_abi));
struct Vector3 HkContactPoint_GetPosition(void * instance) {
	printf("invoke HkContactPoint_GetPosition\n");
	return __PVE_HkContactPoint_GetPosition(instance);
}

void (*__PVE_HkContactPoint_SetPosition)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkContactPoint_SetPosition(void * instance, struct Vector3 value) {
	printf("invoke HkContactPoint_SetPosition\n");
	return __PVE_HkContactPoint_SetPosition(instance, value);
}

struct Vector4 (*__PVE_HkContactPoint_GetNormalAndDistance)(void * instance) __attribute__((ms_abi));
struct Vector4 HkContactPoint_GetNormalAndDistance(void * instance) {
	printf("invoke HkContactPoint_GetNormalAndDistance\n");
	return __PVE_HkContactPoint_GetNormalAndDistance(instance);
}

void (*__PVE_HkContactPoint_SetNormalAndDistance)(void * instance, struct Vector4 value) __attribute__((ms_abi));
void HkContactPoint_SetNormalAndDistance(void * instance, struct Vector4 value) {
	printf("invoke HkContactPoint_SetNormalAndDistance\n");
	return __PVE_HkContactPoint_SetNormalAndDistance(instance, value);
}

struct Vector3 (*__PVE_HkContactPoint_GetNormal)(void * instance) __attribute__((ms_abi));
struct Vector3 HkContactPoint_GetNormal(void * instance) {
	printf("invoke HkContactPoint_GetNormal\n");
	return __PVE_HkContactPoint_GetNormal(instance);
}

void (*__PVE_HkContactPoint_SetNormal)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkContactPoint_SetNormal(void * instance, struct Vector3 value) {
	printf("invoke HkContactPoint_SetNormal\n");
	return __PVE_HkContactPoint_SetNormal(instance, value);
}

float (*__PVE_HkContactPoint_GetDistance)(void * instance) __attribute__((ms_abi));
float HkContactPoint_GetDistance(void * instance) {
	printf("invoke HkContactPoint_GetDistance\n");
	return __PVE_HkContactPoint_GetDistance(instance);
}

void (*__PVE_HkContactPoint_SetDistance)(void * instance, float value) __attribute__((ms_abi));
void HkContactPoint_SetDistance(void * instance, float value) {
	printf("invoke HkContactPoint_SetDistance\n");
	return __PVE_HkContactPoint_SetDistance(instance, value);
}

void (*__PVE_HkContactPoint_Flip)(void * instance) __attribute__((ms_abi));
void HkContactPoint_Flip(void * instance) {
	printf("invoke HkContactPoint_Flip\n");
	return __PVE_HkContactPoint_Flip(instance);
}

void * (*__PVE_HkContactPointEvent_GetBase)(void * instance) __attribute__((ms_abi));
void * HkContactPointEvent_GetBase(void * instance) {
	printf("invoke HkContactPointEvent_GetBase\n");
	return __PVE_HkContactPointEvent_GetBase(instance);
}

int (*__PVE_HkContactPointEvent_IsToi)(void * instance) __attribute__((ms_abi));
int HkContactPointEvent_IsToi(void * instance) {
	printf("invoke HkContactPointEvent_IsToi\n");
	return __PVE_HkContactPointEvent_IsToi(instance);
}

float (*__PVE_HkContactPointEvent_GetSeparatingVelocity)(void * instance) __attribute__((ms_abi));
float HkContactPointEvent_GetSeparatingVelocity(void * instance) {
	printf("invoke HkContactPointEvent_GetSeparatingVelocity\n");
	return __PVE_HkContactPointEvent_GetSeparatingVelocity(instance);
}

void (*__PVE_HkContactPointEvent_SetSeparatingVelocity)(void * instance, float value) __attribute__((ms_abi));
void HkContactPointEvent_SetSeparatingVelocity(void * instance, float value) {
	printf("invoke HkContactPointEvent_SetSeparatingVelocity\n");
	return __PVE_HkContactPointEvent_SetSeparatingVelocity(instance, value);
}

float (*__PVE_HkContactPointEvent_GetRotateNormal)(void * instance) __attribute__((ms_abi));
float HkContactPointEvent_GetRotateNormal(void * instance) {
	printf("invoke HkContactPointEvent_GetRotateNormal\n");
	return __PVE_HkContactPointEvent_GetRotateNormal(instance);
}

void (*__PVE_HkContactPointEvent_SetRotateNormal)(void * instance, float value) __attribute__((ms_abi));
void HkContactPointEvent_SetRotateNormal(void * instance, float value) {
	printf("invoke HkContactPointEvent_SetRotateNormal\n");
	return __PVE_HkContactPointEvent_SetRotateNormal(instance, value);
}

int (*__PVE_HkContactPointEvent_GetEventType)(void * instance) __attribute__((ms_abi));
int HkContactPointEvent_GetEventType(void * instance) {
	printf("invoke HkContactPointEvent_GetEventType\n");
	return __PVE_HkContactPointEvent_GetEventType(instance);
}

void * (*__PVE_HkContactPointEvent_GetContactPoint)(void * instance) __attribute__((ms_abi));
void * HkContactPointEvent_GetContactPoint(void * instance) {
	printf("invoke HkContactPointEvent_GetContactPoint\n");
	return __PVE_HkContactPointEvent_GetContactPoint(instance);
}

void * (*__PVE_HkContactPointEvent_GetContactProperties)(void * instance) __attribute__((ms_abi));
void * HkContactPointEvent_GetContactProperties(void * instance) {
	printf("invoke HkContactPointEvent_GetContactProperties\n");
	return __PVE_HkContactPointEvent_GetContactProperties(instance);
}

int (*__PVE_HkContactPointEvent_GetFiringCallbacksForFullManifold)(void * instance) __attribute__((ms_abi));
int HkContactPointEvent_GetFiringCallbacksForFullManifold(void * instance) {
	printf("invoke HkContactPointEvent_GetFiringCallbacksForFullManifold\n");
	return __PVE_HkContactPointEvent_GetFiringCallbacksForFullManifold(instance);
}

int (*__PVE_HkContactPointEvent_GetFirstCallbackForFullManifold)(void * instance) __attribute__((ms_abi));
int HkContactPointEvent_GetFirstCallbackForFullManifold(void * instance) {
	printf("invoke HkContactPointEvent_GetFirstCallbackForFullManifold\n");
	return __PVE_HkContactPointEvent_GetFirstCallbackForFullManifold(instance);
}

int (*__PVE_HkContactPointEvent_GetLastCallbackForFullManifold)(void * instance) __attribute__((ms_abi));
int HkContactPointEvent_GetLastCallbackForFullManifold(void * instance) {
	printf("invoke HkContactPointEvent_GetLastCallbackForFullManifold\n");
	return __PVE_HkContactPointEvent_GetLastCallbackForFullManifold(instance);
}

short (*__PVE_HkContactPointEvent_GetContactPointId)(void * instance) __attribute__((ms_abi));
short HkContactPointEvent_GetContactPointId(void * instance) {
	printf("invoke HkContactPointEvent_GetContactPointId\n");
	return __PVE_HkContactPointEvent_GetContactPointId(instance);
}

void (*__PVE_HkContactPointEvent_AccessVelocities)(void * instance, int bodyIndex) __attribute__((ms_abi));
void HkContactPointEvent_AccessVelocities(void * instance, int bodyIndex) {
	printf("invoke HkContactPointEvent_AccessVelocities\n");
	return __PVE_HkContactPointEvent_AccessVelocities(instance, bodyIndex);
}

void (*__PVE_HkContactPointEvent_UpdateVelocities)(void * instance, int bodyIndex) __attribute__((ms_abi));
void HkContactPointEvent_UpdateVelocities(void * instance, int bodyIndex) {
	printf("invoke HkContactPointEvent_UpdateVelocities\n");
	return __PVE_HkContactPointEvent_UpdateVelocities(instance, bodyIndex);
}

int (*__PVE_HkContactPointEvent_GetShapeKey)(void * instance, int bodyIdx) __attribute__((ms_abi));
int HkContactPointEvent_GetShapeKey(void * instance, int bodyIdx) {
	printf("invoke HkContactPointEvent_GetShapeKey\n");
	return __PVE_HkContactPointEvent_GetShapeKey(instance, bodyIdx);
}

int (*__PVE_HkContactPointEvent_GetShapeKeyWithShapeID)(void * instance, int bodyIdx, int shapeIdx) __attribute__((ms_abi));
int HkContactPointEvent_GetShapeKeyWithShapeID(void * instance, int bodyIdx, int shapeIdx) {
	printf("invoke HkContactPointEvent_GetShapeKeyWithShapeID\n");
	return __PVE_HkContactPointEvent_GetShapeKeyWithShapeID(instance, bodyIdx, shapeIdx);
}

void (*__PVE_HkContactPointEvent_GetFieldOffsets)(void * separatingVelocityOffset, void * typeOffset, void * propertiesOffset, void * contactPointOffset, void * firingCallbacksForFullManifoldOffset, void * firstCallbackForFullManifoldOffset, void * lastCallbackForFullManifoldOffset) __attribute__((ms_abi));
void HkContactPointEvent_GetFieldOffsets(void * separatingVelocityOffset, void * typeOffset, void * propertiesOffset, void * contactPointOffset, void * firingCallbacksForFullManifoldOffset, void * firstCallbackForFullManifoldOffset, void * lastCallbackForFullManifoldOffset) {
	printf("invoke HkContactPointEvent_GetFieldOffsets\n");
	return __PVE_HkContactPointEvent_GetFieldOffsets(separatingVelocityOffset, typeOffset, propertiesOffset, contactPointOffset, firingCallbacksForFullManifoldOffset, firstCallbackForFullManifoldOffset, lastCallbackForFullManifoldOffset);
}

float (*__PVE_HkContactPointProperties_GetImpulseApplied)(void * instance) __attribute__((ms_abi));
float HkContactPointProperties_GetImpulseApplied(void * instance) {
	printf("invoke HkContactPointProperties_GetImpulseApplied\n");
	return __PVE_HkContactPointProperties_GetImpulseApplied(instance);
}

float (*__PVE_HkContactPointProperties_GetInternalSolverData)(void * instance) __attribute__((ms_abi));
float HkContactPointProperties_GetInternalSolverData(void * instance) {
	printf("invoke HkContactPointProperties_GetInternalSolverData\n");
	return __PVE_HkContactPointProperties_GetInternalSolverData(instance);
}

int (*__PVE_HkContactPointProperties_WasUsed)(void * instance) __attribute__((ms_abi));
int HkContactPointProperties_WasUsed(void * instance) {
	printf("invoke HkContactPointProperties_WasUsed\n");
	return __PVE_HkContactPointProperties_WasUsed(instance);
}

float (*__PVE_HkContactPointProperties_GetFriction)(void * instance) __attribute__((ms_abi));
float HkContactPointProperties_GetFriction(void * instance) {
	printf("invoke HkContactPointProperties_GetFriction\n");
	return __PVE_HkContactPointProperties_GetFriction(instance);
}

void (*__PVE_HkContactPointProperties_SetFriction)(void * instance, float value) __attribute__((ms_abi));
void HkContactPointProperties_SetFriction(void * instance, float value) {
	printf("invoke HkContactPointProperties_SetFriction\n");
	return __PVE_HkContactPointProperties_SetFriction(instance, value);
}

float (*__PVE_HkContactPointProperties_GetRestitution)(void * instance) __attribute__((ms_abi));
float HkContactPointProperties_GetRestitution(void * instance) {
	printf("invoke HkContactPointProperties_GetRestitution\n");
	return __PVE_HkContactPointProperties_GetRestitution(instance);
}

void (*__PVE_HkContactPointProperties_SetRestitution)(void * instance, float value) __attribute__((ms_abi));
void HkContactPointProperties_SetRestitution(void * instance, float value) {
	printf("invoke HkContactPointProperties_SetRestitution\n");
	return __PVE_HkContactPointProperties_SetRestitution(instance, value);
}

int (*__PVE_HkContactPointProperties_IsPotential)(void * instance) __attribute__((ms_abi));
int HkContactPointProperties_IsPotential(void * instance) {
	printf("invoke HkContactPointProperties_IsPotential\n");
	return __PVE_HkContactPointProperties_IsPotential(instance);
}

float (*__PVE_HkContactPointProperties_GetMaxImpulsePerStep)(void * instance) __attribute__((ms_abi));
float HkContactPointProperties_GetMaxImpulsePerStep(void * instance) {
	printf("invoke HkContactPointProperties_GetMaxImpulsePerStep\n");
	return __PVE_HkContactPointProperties_GetMaxImpulsePerStep(instance);
}

void (*__PVE_HkContactPointProperties_SetMaxImpulsePerStep)(void * instance, float value) __attribute__((ms_abi));
void HkContactPointProperties_SetMaxImpulsePerStep(void * instance, float value) {
	printf("invoke HkContactPointProperties_SetMaxImpulsePerStep\n");
	return __PVE_HkContactPointProperties_SetMaxImpulsePerStep(instance, value);
}

float (*__PVE_HkContactPointProperties_GetMaxImpulse)(void * instance) __attribute__((ms_abi));
float HkContactPointProperties_GetMaxImpulse(void * instance) {
	printf("invoke HkContactPointProperties_GetMaxImpulse\n");
	return __PVE_HkContactPointProperties_GetMaxImpulse(instance);
}

void (*__PVE_HkContactPointProperties_SetMaxImpulse)(void * instance, float value) __attribute__((ms_abi));
void HkContactPointProperties_SetMaxImpulse(void * instance, float value) {
	printf("invoke HkContactPointProperties_SetMaxImpulse\n");
	return __PVE_HkContactPointProperties_SetMaxImpulse(instance, value);
}

int (*__PVE_HkContactPointProperties_GetIsDisabled)(void * instance) __attribute__((ms_abi));
int HkContactPointProperties_GetIsDisabled(void * instance) {
	printf("invoke HkContactPointProperties_GetIsDisabled\n");
	return __PVE_HkContactPointProperties_GetIsDisabled(instance);
}

void (*__PVE_HkContactPointProperties_SetIsDisabled)(void * instance, int value) __attribute__((ms_abi));
void HkContactPointProperties_SetIsDisabled(void * instance, int value) {
	printf("invoke HkContactPointProperties_SetIsDisabled\n");
	return __PVE_HkContactPointProperties_SetIsDisabled(instance, value);
}

int (*__PVE_HkContactPointProperties_GetIsNew)(void * instance) __attribute__((ms_abi));
int HkContactPointProperties_GetIsNew(void * instance) {
	printf("invoke HkContactPointProperties_GetIsNew\n");
	return __PVE_HkContactPointProperties_GetIsNew(instance);
}

void (*__PVE_HkContactPointProperties_SetIsNew)(void * instance, int value) __attribute__((ms_abi));
void HkContactPointProperties_SetIsNew(void * instance, int value) {
	printf("invoke HkContactPointProperties_SetIsNew\n");
	return __PVE_HkContactPointProperties_SetIsNew(instance, value);
}

int (*__PVE_HkContactPointProperties_GetUserData)(void * instance) __attribute__((ms_abi));
int HkContactPointProperties_GetUserData(void * instance) {
	printf("invoke HkContactPointProperties_GetUserData\n");
	return __PVE_HkContactPointProperties_GetUserData(instance);
}

void (*__PVE_HkContactPointProperties_SetUserData)(void * instance, int value) __attribute__((ms_abi));
void HkContactPointProperties_SetUserData(void * instance, int value) {
	printf("invoke HkContactPointProperties_SetUserData\n");
	return __PVE_HkContactPointProperties_SetUserData(instance, value);
}

void (*__PVE_HkContactPointProperties_GetFieldOffsets)(void * userDataOffset) __attribute__((ms_abi));
void HkContactPointProperties_GetFieldOffsets(void * userDataOffset) {
	printf("invoke HkContactPointProperties_GetFieldOffsets\n");
	return __PVE_HkContactPointProperties_GetFieldOffsets(userDataOffset);
}

void * (*__PVE_HkContactSoundListener_Create)(void * onContact) __attribute__((ms_abi));
void * HkContactSoundListener_Create(void * onContact) {
	printf("invoke HkContactSoundListener_Create\n");
	return __PVE_HkContactSoundListener_Create(_PVE_Trampoline_Havok_HkContactSoundListener_ContactSoundHandler(onContact));
}

void (*__PVE_HkEntity_AddActivationListener)(void * instance, void * listener) __attribute__((ms_abi));
void HkEntity_AddActivationListener(void * instance, void * listener) {
	printf("invoke HkEntity_AddActivationListener\n");
	return __PVE_HkEntity_AddActivationListener(instance, listener);
}

void (*__PVE_HkEntity_RemoveActivationListener)(void * instance, void * listener) __attribute__((ms_abi));
void HkEntity_RemoveActivationListener(void * instance, void * listener) {
	printf("invoke HkEntity_RemoveActivationListener\n");
	return __PVE_HkEntity_RemoveActivationListener(instance, listener);
}

void (*__PVE_HKEntity_AddEntityListener)(void * instance, void * listener) __attribute__((ms_abi));
void HKEntity_AddEntityListener(void * instance, void * listener) {
	printf("invoke HKEntity_AddEntityListener\n");
	return __PVE_HKEntity_AddEntityListener(instance, listener);
}

void (*__PVE_HKEntity_RemoveEntityListener)(void * instance, void * listener) __attribute__((ms_abi));
void HKEntity_RemoveEntityListener(void * instance, void * listener) {
	printf("invoke HKEntity_RemoveEntityListener\n");
	return __PVE_HKEntity_RemoveEntityListener(instance, listener);
}

void (*__PVE_HkEntity_SetContactListener)(void * instance, void * listener, int value) __attribute__((ms_abi));
void HkEntity_SetContactListener(void * instance, void * listener, int value) {
	printf("invoke HkEntity_SetContactListener\n");
	return __PVE_HkEntity_SetContactListener(instance, listener, value);
}

int (*__PVE_HkEntity_GetQuality)(void * instance) __attribute__((ms_abi));
int HkEntity_GetQuality(void * instance) {
	printf("invoke HkEntity_GetQuality\n");
	return __PVE_HkEntity_GetQuality(instance);
}

void (*__PVE_HkEntity_SetQuality)(void * instance, int value) __attribute__((ms_abi));
void HkEntity_SetQuality(void * instance, int value) {
	printf("invoke HkEntity_SetQuality\n");
	return __PVE_HkEntity_SetQuality(instance, value);
}

int (*__PVE_HkEntity_IsFixed)(void * instance) __attribute__((ms_abi));
int HkEntity_IsFixed(void * instance) {
	printf("invoke HkEntity_IsFixed\n");
	return __PVE_HkEntity_IsFixed(instance);
}

int (*__PVE_HkEntity_IsFixedOrKeyframed)(void * instance) __attribute__((ms_abi));
int HkEntity_IsFixedOrKeyframed(void * instance) {
	printf("invoke HkEntity_IsFixedOrKeyframed\n");
	return __PVE_HkEntity_IsFixedOrKeyframed(instance);
}

int (*__PVE_HkRigidBody_GetMotionType)(void * instance) __attribute__((ms_abi));
int HkRigidBody_GetMotionType(void * instance) {
	printf("invoke HkRigidBody_GetMotionType\n");
	return __PVE_HkRigidBody_GetMotionType(instance);
}

int (*__PVE_HkEntity_GetContactPointCallbackDelay)(void * instance) __attribute__((ms_abi));
int HkEntity_GetContactPointCallbackDelay(void * instance) {
	printf("invoke HkEntity_GetContactPointCallbackDelay\n");
	return __PVE_HkEntity_GetContactPointCallbackDelay(instance);
}

void (*__PVE_HkEntity_SetContactPointCallbackDelay)(void * instance, int value) __attribute__((ms_abi));
void HkEntity_SetContactPointCallbackDelay(void * instance, int value) {
	printf("invoke HkEntity_SetContactPointCallbackDelay\n");
	return __PVE_HkEntity_SetContactPointCallbackDelay(instance, value);
}

void (*__PVE_HkEntity_SetProperty)(void * instance, int key, float v) __attribute__((ms_abi));
void HkEntity_SetProperty(void * instance, int key, float v) {
	printf("invoke HkEntity_SetProperty\n");
	return __PVE_HkEntity_SetProperty(instance, key, v);
}

int (*__PVE_HkEntity_HasProperty)(void * instance, int key) __attribute__((ms_abi));
int HkEntity_HasProperty(void * instance, int key) {
	printf("invoke HkEntity_HasProperty\n");
	return __PVE_HkEntity_HasProperty(instance, key);
}

void (*__PVE_HkEntity_RemoveProperty)(void * instance, int key) __attribute__((ms_abi));
void HkEntity_RemoveProperty(void * instance, int key) {
	printf("invoke HkEntity_RemoveProperty\n");
	return __PVE_HkEntity_RemoveProperty(instance, key);
}

struct Quaternion (*__PVE_HkRigidBody_GetRotation)(void * instance) __attribute__((ms_abi));
struct Quaternion HkRigidBody_GetRotation(void * instance) {
	printf("invoke HkRigidBody_GetRotation\n");
	return __PVE_HkRigidBody_GetRotation(instance);
}

void (*__PVE_HkRigidBody_SetRotation)(void * instance, struct Quaternion value) __attribute__((ms_abi));
void HkRigidBody_SetRotation(void * instance, struct Quaternion value) {
	printf("invoke HkRigidBody_SetRotation\n");
	return __PVE_HkRigidBody_SetRotation(instance, value);
}

struct Vector3 (*__PVE_HkRigidBody_GetPosition)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBody_GetPosition(void * instance) {
	printf("invoke HkRigidBody_GetPosition\n");
	return __PVE_HkRigidBody_GetPosition(instance);
}

void (*__PVE_HkRigidBody_SetPosition)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkRigidBody_SetPosition(void * instance, struct Vector3 value) {
	printf("invoke HkRigidBody_SetPosition\n");
	return __PVE_HkRigidBody_SetPosition(instance, value);
}

void (*__PVE_HkRigidBody_Activate)(void * instance) __attribute__((ms_abi));
void HkRigidBody_Activate(void * instance) {
	printf("invoke HkRigidBody_Activate\n");
	return __PVE_HkRigidBody_Activate(instance);
}

void (*__PVE_HkRigidBody_ActivateAsCriticalOperation)(void * instance) __attribute__((ms_abi));
void HkRigidBody_ActivateAsCriticalOperation(void * instance) {
	printf("invoke HkRigidBody_ActivateAsCriticalOperation\n");
	return __PVE_HkRigidBody_ActivateAsCriticalOperation(instance);
}

void (*__PVE_HkRigidBody_Deactivate)(void * instance) __attribute__((ms_abi));
void HkRigidBody_Deactivate(void * instance) {
	printf("invoke HkRigidBody_Deactivate\n");
	return __PVE_HkRigidBody_Deactivate(instance);
}

void (*__PVE_HkRigidBody_UpdateMotionType)(void * instance, int type) __attribute__((ms_abi));
void HkRigidBody_UpdateMotionType(void * instance, int type) {
	printf("invoke HkRigidBody_UpdateMotionType\n");
	return __PVE_HkRigidBody_UpdateMotionType(instance, type);
}

int (*__PVE_HkRigidBody_GetIsActive)(void * instance) __attribute__((ms_abi));
int HkRigidBody_GetIsActive(void * instance) {
	printf("invoke HkRigidBody_GetIsActive\n");
	return __PVE_HkRigidBody_GetIsActive(instance);
}

void (*__PVE_HkRigidBody_RequestDeactivation)(void * instance) __attribute__((ms_abi));
void HkRigidBody_RequestDeactivation(void * instance) {
	printf("invoke HkRigidBody_RequestDeactivation\n");
	return __PVE_HkRigidBody_RequestDeactivation(instance);
}

struct Vector3 (*__PVE_HkRigidBody_GetLinearVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBody_GetLinearVelocity(void * instance) {
	printf("invoke HkRigidBody_GetLinearVelocity\n");
	return __PVE_HkRigidBody_GetLinearVelocity(instance);
}

void (*__PVE_HkRigidBody_SetLinearVelocity)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkRigidBody_SetLinearVelocity(void * instance, struct Vector3 value) {
	printf("invoke HkRigidBody_SetLinearVelocity\n");
	return __PVE_HkRigidBody_SetLinearVelocity(instance, value);
}

struct Vector3 (*__PVE_HkRigidBody_GetAngularVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBody_GetAngularVelocity(void * instance) {
	printf("invoke HkRigidBody_GetAngularVelocity\n");
	return __PVE_HkRigidBody_GetAngularVelocity(instance);
}

void (*__PVE_HkRigidBody_SetAngularVelocity)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkRigidBody_SetAngularVelocity(void * instance, struct Vector3 value) {
	printf("invoke HkRigidBody_SetAngularVelocity\n");
	return __PVE_HkRigidBody_SetAngularVelocity(instance, value);
}

void (*__PVE_HkEntity_GetFieldOffsets)(void * userDataOffset, void * transformOffset, void * rotationOffset, void * linearVelocityOffset, void * angularVelocityOffset, void * motionTypeOffset, void * simulationIslandOffset, void * worldOffset) __attribute__((ms_abi));
void HkEntity_GetFieldOffsets(void * userDataOffset, void * transformOffset, void * rotationOffset, void * linearVelocityOffset, void * angularVelocityOffset, void * motionTypeOffset, void * simulationIslandOffset, void * worldOffset) {
	printf("invoke HkEntity_GetFieldOffsets\n");
	return __PVE_HkEntity_GetFieldOffsets(userDataOffset, transformOffset, rotationOffset, linearVelocityOffset, angularVelocityOffset, motionTypeOffset, simulationIslandOffset, worldOffset);
}

void * (*__PVE_HkEntityListener_Create)(void * onAdd, void * onRemove, void * onDelete, void * onShapeChange, void * onMotionTypeChange) __attribute__((ms_abi));
void * HkEntityListener_Create(void * onAdd, void * onRemove, void * onDelete, void * onShapeChange, void * onMotionTypeChange) {
	printf("invoke HkEntityListener_Create\n");
	return __PVE_HkEntityListener_Create(_PVE_Trampoline_Havok_HkEntityListener_OnAddCpp(onAdd), _PVE_Trampoline_Havok_HkEntityListener_OnRemoveCpp(onRemove), _PVE_Trampoline_Havok_HkEntityListener_OnDeleteCpp(onDelete), _PVE_Trampoline_Havok_HkEntityListener_OnShapeChangeCpp(onShapeChange), _PVE_Trampoline_Havok_HkEntityListener_OnMotionTypeChangeCpp(onMotionTypeChange));
}

void (*__PVE_HkEntityListener_Release)(void * entityListener) __attribute__((ms_abi));
void HkEntityListener_Release(void * entityListener) {
	printf("invoke HkEntityListener_Release\n");
	return __PVE_HkEntityListener_Release(entityListener);
}

void (*__PVE_HkGlobal_ReleasePtr)(void * ptr) __attribute__((ms_abi));
void HkGlobal_ReleasePtr(void * ptr) {
	printf("invoke HkGlobal_ReleasePtr\n");
	return __PVE_HkGlobal_ReleasePtr(ptr);
}

void (*__PVE_HkGlobal_ReleaseString)(void * ptr) __attribute__((ms_abi));
void HkGlobal_ReleaseString(void * ptr) {
	printf("invoke HkGlobal_ReleaseString\n");
	return __PVE_HkGlobal_ReleaseString(ptr);
}

void (*__PVE_HkGlobal_ReleaseArrayPtr)(void * ptr) __attribute__((ms_abi));
void HkGlobal_ReleaseArrayPtr(void * ptr) {
	printf("invoke HkGlobal_ReleaseArrayPtr\n");
	return __PVE_HkGlobal_ReleaseArrayPtr(ptr);
}

void * (*__PVE_HkJobQueue_Create)(void * outThreadCount) __attribute__((ms_abi));
void * HkJobQueue_Create(void * outThreadCount) {
	printf("invoke HkJobQueue_Create\n");
	return __PVE_HkJobQueue_Create(outThreadCount);
}

void * (*__PVE_HkJobQueue_CreateWithNumThreads)(int threadCount) __attribute__((ms_abi));
void * HkJobQueue_CreateWithNumThreads(int threadCount) {
	printf("invoke HkJobQueue_CreateWithNumThreads\n");
	return __PVE_HkJobQueue_CreateWithNumThreads(threadCount);
}

void (*__PVE_HkJobQueue_Release)(void * instance) __attribute__((ms_abi));
void HkJobQueue_Release(void * instance) {
	printf("invoke HkJobQueue_Release\n");
	return __PVE_HkJobQueue_Release(instance);
}

int (*__PVE_HkJobQueue_GetWaitPolicy)(void * jobQueue) __attribute__((ms_abi));
int HkJobQueue_GetWaitPolicy(void * jobQueue) {
	printf("invoke HkJobQueue_GetWaitPolicy\n");
	return __PVE_HkJobQueue_GetWaitPolicy(jobQueue);
}

void (*__PVE_HkJobQueue_SetWaitPolicy)(void * jobQueue, int value) __attribute__((ms_abi));
void HkJobQueue_SetWaitPolicy(void * jobQueue, int value) {
	printf("invoke HkJobQueue_SetWaitPolicy\n");
	return __PVE_HkJobQueue_SetWaitPolicy(jobQueue, value);
}

int (*__PVE_HkJobQueue_GetMasterThreadFinishingFlags)(void * jobQueue) __attribute__((ms_abi));
int HkJobQueue_GetMasterThreadFinishingFlags(void * jobQueue) {
	printf("invoke HkJobQueue_GetMasterThreadFinishingFlags\n");
	return __PVE_HkJobQueue_GetMasterThreadFinishingFlags(jobQueue);
}

void (*__PVE_HkJobQueue_SetMasterThreadFinishingFlags)(void * jobQueue, int value) __attribute__((ms_abi));
void HkJobQueue_SetMasterThreadFinishingFlags(void * jobQueue, int value) {
	printf("invoke HkJobQueue_SetMasterThreadFinishingFlags\n");
	return __PVE_HkJobQueue_SetMasterThreadFinishingFlags(jobQueue, value);
}

void (*__PVE_HkJobQueue_ProcessAllJobs)(void * jobQueue) __attribute__((ms_abi));
void HkJobQueue_ProcessAllJobs(void * jobQueue) {
	printf("invoke HkJobQueue_ProcessAllJobs\n");
	return __PVE_HkJobQueue_ProcessAllJobs(jobQueue);
}

void * (*__PVE_HkJobThreadPool_Create)(void * outThreadCount) __attribute__((ms_abi));
void * HkJobThreadPool_Create(void * outThreadCount) {
	printf("invoke HkJobThreadPool_Create\n");
	return __PVE_HkJobThreadPool_Create(outThreadCount);
}

void * (*__PVE_HkJobThreadPool_CreateWithNumThreads)(int threadCount) __attribute__((ms_abi));
void * HkJobThreadPool_CreateWithNumThreads(int threadCount) {
	printf("invoke HkJobThreadPool_CreateWithNumThreads\n");
	return __PVE_HkJobThreadPool_CreateWithNumThreads(threadCount);
}

void (*__PVE_HkJobThreadPool_RemoveReference)(void * instance) __attribute__((ms_abi));
void HkJobThreadPool_RemoveReference(void * instance) {
	printf("invoke HkJobThreadPool_RemoveReference\n");
	return __PVE_HkJobThreadPool_RemoveReference(instance);
}

void (*__PVE_HkJobThreadPool_RunOnEachWorker)(void * instance, void * action, void * data) __attribute__((ms_abi));
void HkJobThreadPool_RunOnEachWorker(void * instance, void * action, void * data) {
	printf("invoke HkJobThreadPool_RunOnEachWorker\n");
	return __PVE_HkJobThreadPool_RunOnEachWorker(instance, _PVE_Trampoline_Havok_HkJobThreadPool_ThreadAction(action), data);
}

void (*__PVE_HkJobThreadPool_ExecuteJobQueue)(void * instance, void * jobQueue) __attribute__((ms_abi));
void HkJobThreadPool_ExecuteJobQueue(void * instance, void * jobQueue) {
	printf("invoke HkJobThreadPool_ExecuteJobQueue\n");
	return __PVE_HkJobThreadPool_ExecuteJobQueue(instance, jobQueue);
}

int (*__PVE_HkJobThreadPool_GetThisThreadIndex)(void * instance) __attribute__((ms_abi));
int HkJobThreadPool_GetThisThreadIndex(void * instance) {
	printf("invoke HkJobThreadPool_GetThisThreadIndex\n");
	return __PVE_HkJobThreadPool_GetThisThreadIndex(instance);
}

void (*__PVE_HkJobThreadPool_WaitForCompletion)(void * instance) __attribute__((ms_abi));
void HkJobThreadPool_WaitForCompletion(void * instance) {
	printf("invoke HkJobThreadPool_WaitForCompletion\n");
	return __PVE_HkJobThreadPool_WaitForCompletion(instance);
}

void (*__PVE_HkJobThreadPool_ClearTimerData)(void * instance) __attribute__((ms_abi));
void HkJobThreadPool_ClearTimerData(void * instance) {
	printf("invoke HkJobThreadPool_ClearTimerData\n");
	return __PVE_HkJobThreadPool_ClearTimerData(instance);
}

void (*__PVE_HkMotion_SetWorldMatrix)(void * instance, struct Matrix m) __attribute__((ms_abi));
void HkMotion_SetWorldMatrix(void * instance, struct Matrix m) {
	printf("invoke HkMotion_SetWorldMatrix\n");
	return __PVE_HkMotion_SetWorldMatrix(instance, m);
}

int (*__PVE_HkMotion_GetDeactivationClass)(void * instance) __attribute__((ms_abi));
int HkMotion_GetDeactivationClass(void * instance) {
	printf("invoke HkMotion_GetDeactivationClass\n");
	return __PVE_HkMotion_GetDeactivationClass(instance);
}

void (*__PVE_HkMotion_SetDeactivationClass)(void * instance, int value) __attribute__((ms_abi));
void HkMotion_SetDeactivationClass(void * instance, int value) {
	printf("invoke HkMotion_SetDeactivationClass\n");
	return __PVE_HkMotion_SetDeactivationClass(instance, value);
}

void (*__PVE_HkReferenceObject_AddReference)(void * instance) __attribute__((ms_abi));
void HkReferenceObject_AddReference(void * instance) {
	printf("invoke HkReferenceObject_AddReference\n");
	return __PVE_HkReferenceObject_AddReference(instance);
}

void (*__PVE_HkReferenceObject_RemoveReference)(void * instance) __attribute__((ms_abi));
void HkReferenceObject_RemoveReference(void * instance) {
	printf("invoke HkReferenceObject_RemoveReference\n");
	return __PVE_HkReferenceObject_RemoveReference(instance);
}

int (*__PVE_HkReferenceObject_IsValid)(void * instance) __attribute__((ms_abi));
int HkReferenceObject_IsValid(void * instance) {
	printf("invoke HkReferenceObject_IsValid\n");
	return __PVE_HkReferenceObject_IsValid(instance);
}

void (*__PVE_HkReferenceObject_DebugRemoveRef)(void * instance) __attribute__((ms_abi));
void HkReferenceObject_DebugRemoveRef(void * instance) {
	printf("invoke HkReferenceObject_DebugRemoveRef\n");
	return __PVE_HkReferenceObject_DebugRemoveRef(instance);
}

int (*__PVE_HkReferenceObject_ReferenceCount)(void * instance) __attribute__((ms_abi));
int HkReferenceObject_ReferenceCount(void * instance) {
	printf("invoke HkReferenceObject_ReferenceCount\n");
	return __PVE_HkReferenceObject_ReferenceCount(instance);
}

void * (*__PVE_HkRigidBody_Create)(void * bodyInfo) __attribute__((ms_abi));
void * HkRigidBody_Create(void * bodyInfo) {
	printf("invoke HkRigidBody_Create\n");
	return __PVE_HkRigidBody_Create(bodyInfo);
}

void * (*__PVE_HkRigidBody_CreateWithCustomVelocity)(void * bodyInfo) __attribute__((ms_abi));
void * HkRigidBody_CreateWithCustomVelocity(void * bodyInfo) {
	printf("invoke HkRigidBody_CreateWithCustomVelocity\n");
	return __PVE_HkRigidBody_CreateWithCustomVelocity(bodyInfo);
}

void (*__PVE_HkRigidBody_SetNumShapeKeysInContactPointProperties)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBody_SetNumShapeKeysInContactPointProperties(void * instance, int value) {
	printf("invoke HkRigidBody_SetNumShapeKeysInContactPointProperties\n");
	return __PVE_HkRigidBody_SetNumShapeKeysInContactPointProperties(instance, value);
}

int (*__PVE_HkRigidBody_GetResponseModifiers)(void * instance) __attribute__((ms_abi));
int HkRigidBody_GetResponseModifiers(void * instance) {
	printf("invoke HkRigidBody_GetResponseModifiers\n");
	return __PVE_HkRigidBody_GetResponseModifiers(instance);
}

void (*__PVE_HkRigidBody_SetResponseModifiers)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBody_SetResponseModifiers(void * instance, int value) {
	printf("invoke HkRigidBody_SetResponseModifiers\n");
	return __PVE_HkRigidBody_SetResponseModifiers(instance, value);
}

void * (*__PVE_HkRigidBody_GetShape)(void * instance) __attribute__((ms_abi));
void * HkRigidBody_GetShape(void * instance) {
	printf("invoke HkRigidBody_GetShape\n");
	return __PVE_HkRigidBody_GetShape(instance);
}

int (*__PVE_HkRigidBody_SetShape)(void * instance, void * shape) __attribute__((ms_abi));
int HkRigidBody_SetShape(void * instance, void * shape) {
	printf("invoke HkRigidBody_SetShape\n");
	return __PVE_HkRigidBody_SetShape(instance, shape);
}

int (*__PVE_HkRigidBody_UpdateShape)(void * instance) __attribute__((ms_abi));
int HkRigidBody_UpdateShape(void * instance) {
	printf("invoke HkRigidBody_UpdateShape\n");
	return __PVE_HkRigidBody_UpdateShape(instance);
}

struct Matrix (*__PVE_HkRigidBody_PredictRigidBodyMatrix)(void * instance, float deltaTime, void * world) __attribute__((ms_abi));
struct Matrix HkRigidBody_PredictRigidBodyMatrix(void * instance, float deltaTime, void * world) {
	printf("invoke HkRigidBody_PredictRigidBodyMatrix\n");
	return __PVE_HkRigidBody_PredictRigidBodyMatrix(instance, deltaTime, world);
}

void (*__PVE_HkRigidBody_SetMassProperties)(void * instance, struct HkMassProperties properties) __attribute__((ms_abi));
void HkRigidBody_SetMassProperties(void * instance, struct HkMassProperties properties) {
	printf("invoke HkRigidBody_SetMassProperties\n");
	return __PVE_HkRigidBody_SetMassProperties(instance, properties);
}

void (*__PVE_HkRigidBody_SetWorldMatrix)(void * instance, struct Matrix m) __attribute__((ms_abi));
void HkRigidBody_SetWorldMatrix(void * instance, struct Matrix m) {
	printf("invoke HkRigidBody_SetWorldMatrix\n");
	return __PVE_HkRigidBody_SetWorldMatrix(instance, m);
}

void (*__PVE_HkRigidBody_SetTransform)(void * instance, struct Matrix m) __attribute__((ms_abi));
void HkRigidBody_SetTransform(void * instance, struct Matrix m) {
	printf("invoke HkRigidBody_SetTransform\n");
	return __PVE_HkRigidBody_SetTransform(instance, m);
}

int (*__PVE_HkRigidBody_GetEnableDeactivation)(void * instance) __attribute__((ms_abi));
int HkRigidBody_GetEnableDeactivation(void * instance) {
	printf("invoke HkRigidBody_GetEnableDeactivation\n");
	return __PVE_HkRigidBody_GetEnableDeactivation(instance);
}

void (*__PVE_HkRigidBody_SetEnableDeactivation)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBody_SetEnableDeactivation(void * instance, int value) {
	printf("invoke HkRigidBody_SetEnableDeactivation\n");
	return __PVE_HkRigidBody_SetEnableDeactivation(instance, value);
}

int (*__PVE_HkRigidBody_GetMarkedForVelocityRecompute)(void * instance) __attribute__((ms_abi));
int HkRigidBody_GetMarkedForVelocityRecompute(void * instance) {
	printf("invoke HkRigidBody_GetMarkedForVelocityRecompute\n");
	return __PVE_HkRigidBody_GetMarkedForVelocityRecompute(instance);
}

void (*__PVE_HkRigidBody_SetMarkedForVelocityRecompute)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBody_SetMarkedForVelocityRecompute(void * instance, int value) {
	printf("invoke HkRigidBody_SetMarkedForVelocityRecompute\n");
	return __PVE_HkRigidBody_SetMarkedForVelocityRecompute(instance, value);
}

void * (*__PVE_HkRigidBody_GetMotion)(void * instance) __attribute__((ms_abi));
void * HkRigidBody_GetMotion(void * instance) {
	printf("invoke HkRigidBody_GetMotion\n");
	return __PVE_HkRigidBody_GetMotion(instance);
}

float (*__PVE_HkRigidBody_GetMass)(void * instance) __attribute__((ms_abi));
float HkRigidBody_GetMass(void * instance) {
	printf("invoke HkRigidBody_GetMass\n");
	return __PVE_HkRigidBody_GetMass(instance);
}

void (*__PVE_HkRigidBody_SetMass)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBody_SetMass(void * instance, float value) {
	printf("invoke HkRigidBody_SetMass\n");
	return __PVE_HkRigidBody_SetMass(instance, value);
}

struct Vector3 (*__PVE_HkRigidBody_GetCenterOfMassLocal)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBody_GetCenterOfMassLocal(void * instance) {
	printf("invoke HkRigidBody_GetCenterOfMassLocal\n");
	return __PVE_HkRigidBody_GetCenterOfMassLocal(instance);
}

void (*__PVE_HkRigidBody_SetCenterOfMassLocal)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkRigidBody_SetCenterOfMassLocal(void * instance, struct Vector3 value) {
	printf("invoke HkRigidBody_SetCenterOfMassLocal\n");
	return __PVE_HkRigidBody_SetCenterOfMassLocal(instance, value);
}

struct Matrix (*__PVE_HkRigidBody_GetInertiaTensor)(void * instance) __attribute__((ms_abi));
struct Matrix HkRigidBody_GetInertiaTensor(void * instance) {
	printf("invoke HkRigidBody_GetInertiaTensor\n");
	return __PVE_HkRigidBody_GetInertiaTensor(instance);
}

void (*__PVE_HkRigidBody_SetInertiaTensor)(void * instance, struct Matrix value) __attribute__((ms_abi));
void HkRigidBody_SetInertiaTensor(void * instance, struct Matrix value) {
	printf("invoke HkRigidBody_SetInertiaTensor\n");
	return __PVE_HkRigidBody_SetInertiaTensor(instance, value);
}

struct Matrix (*__PVE_HkRigidBody_GetInverseInertiaTensor)(void * instance) __attribute__((ms_abi));
struct Matrix HkRigidBody_GetInverseInertiaTensor(void * instance) {
	printf("invoke HkRigidBody_GetInverseInertiaTensor\n");
	return __PVE_HkRigidBody_GetInverseInertiaTensor(instance);
}

void (*__PVE_HkRigidBody_SetInverseInertiaTensor)(void * instance, struct Matrix value) __attribute__((ms_abi));
void HkRigidBody_SetInverseInertiaTensor(void * instance, struct Matrix value) {
	printf("invoke HkRigidBody_SetInverseInertiaTensor\n");
	return __PVE_HkRigidBody_SetInverseInertiaTensor(instance, value);
}

struct Vector3 (*__PVE_HkRigidBody_GetCenterOfMassWorld)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBody_GetCenterOfMassWorld(void * instance) {
	printf("invoke HkRigidBody_GetCenterOfMassWorld\n");
	return __PVE_HkRigidBody_GetCenterOfMassWorld(instance);
}

int (*__PVE_HkRigidBody_GetCustomVelocity)(void * instance, void * velocity) __attribute__((ms_abi));
int HkRigidBody_GetCustomVelocity(void * instance, void * velocity) {
	printf("invoke HkRigidBody_GetCustomVelocity\n");
	return __PVE_HkRigidBody_GetCustomVelocity(instance, velocity);
}

void (*__PVE_HkRigidBody_SetCustomVelocity)(void * instance, struct Vector3 value, int valid) __attribute__((ms_abi));
void HkRigidBody_SetCustomVelocity(void * instance, struct Vector3 value, int valid) {
	printf("invoke HkRigidBody_SetCustomVelocity\n");
	return __PVE_HkRigidBody_SetCustomVelocity(instance, value, valid);
}

struct Vector4 (*__PVE_HkRigidBody_GetDeltaAngle)(void * instance) __attribute__((ms_abi));
struct Vector4 HkRigidBody_GetDeltaAngle(void * instance) {
	printf("invoke HkRigidBody_GetDeltaAngle\n");
	return __PVE_HkRigidBody_GetDeltaAngle(instance);
}

float (*__PVE_HkRigidBody_GetLinearDamping)(void * instance) __attribute__((ms_abi));
float HkRigidBody_GetLinearDamping(void * instance) {
	printf("invoke HkRigidBody_GetLinearDamping\n");
	return __PVE_HkRigidBody_GetLinearDamping(instance);
}

void (*__PVE_HkRigidBody_SetLinearDamping)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBody_SetLinearDamping(void * instance, float value) {
	printf("invoke HkRigidBody_SetLinearDamping\n");
	return __PVE_HkRigidBody_SetLinearDamping(instance, value);
}

float (*__PVE_HkRigidBody_GetAngularDamping)(void * instance) __attribute__((ms_abi));
float HkRigidBody_GetAngularDamping(void * instance) {
	printf("invoke HkRigidBody_GetAngularDamping\n");
	return __PVE_HkRigidBody_GetAngularDamping(instance);
}

void (*__PVE_HkRigidBody_SetAngularDamping)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBody_SetAngularDamping(void * instance, float value) {
	printf("invoke HkRigidBody_SetAngularDamping\n");
	return __PVE_HkRigidBody_SetAngularDamping(instance, value);
}

float (*__PVE_HkRigidBody_GetMaxLinearVelocity)(void * instance) __attribute__((ms_abi));
float HkRigidBody_GetMaxLinearVelocity(void * instance) {
	printf("invoke HkRigidBody_GetMaxLinearVelocity\n");
	return __PVE_HkRigidBody_GetMaxLinearVelocity(instance);
}

void (*__PVE_HkRigidBody_SetMaxLinearVelocity)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBody_SetMaxLinearVelocity(void * instance, float value) {
	printf("invoke HkRigidBody_SetMaxLinearVelocity\n");
	return __PVE_HkRigidBody_SetMaxLinearVelocity(instance, value);
}

float (*__PVE_HkRigidBody_GetMaxAngularVelocity)(void * instance) __attribute__((ms_abi));
float HkRigidBody_GetMaxAngularVelocity(void * instance) {
	printf("invoke HkRigidBody_GetMaxAngularVelocity\n");
	return __PVE_HkRigidBody_GetMaxAngularVelocity(instance);
}

void (*__PVE_HkRigidBody_SetMaxAngularVelocity)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBody_SetMaxAngularVelocity(void * instance, float value) {
	printf("invoke HkRigidBody_SetMaxAngularVelocity\n");
	return __PVE_HkRigidBody_SetMaxAngularVelocity(instance, value);
}

float (*__PVE_HkRigidBody_GetAllowedPenetrationDepth)(void * instance) __attribute__((ms_abi));
float HkRigidBody_GetAllowedPenetrationDepth(void * instance) {
	printf("invoke HkRigidBody_GetAllowedPenetrationDepth\n");
	return __PVE_HkRigidBody_GetAllowedPenetrationDepth(instance);
}

void (*__PVE_HkRigidBody_SetAllowedPenetrationDepth)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBody_SetAllowedPenetrationDepth(void * instance, float value) {
	printf("invoke HkRigidBody_SetAllowedPenetrationDepth\n");
	return __PVE_HkRigidBody_SetAllowedPenetrationDepth(instance, value);
}

float (*__PVE_HkRigidBody_GetFriction)(void * instance) __attribute__((ms_abi));
float HkRigidBody_GetFriction(void * instance) {
	printf("invoke HkRigidBody_GetFriction\n");
	return __PVE_HkRigidBody_GetFriction(instance);
}

void (*__PVE_HkRigidBody_SetFriction)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBody_SetFriction(void * instance, float value) {
	printf("invoke HkRigidBody_SetFriction\n");
	return __PVE_HkRigidBody_SetFriction(instance, value);
}

float (*__PVE_HkRigidBody_GetRestitution)(void * instance) __attribute__((ms_abi));
float HkRigidBody_GetRestitution(void * instance) {
	printf("invoke HkRigidBody_GetRestitution\n");
	return __PVE_HkRigidBody_GetRestitution(instance);
}

void (*__PVE_HkRigidBody_SetRestitution)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBody_SetRestitution(void * instance, float value) {
	printf("invoke HkRigidBody_SetRestitution\n");
	return __PVE_HkRigidBody_SetRestitution(instance, value);
}

void (*__PVE_HkRigidBody_ApplyLinearImpulse)(void * instance, struct Vector3 impulse) __attribute__((ms_abi));
void HkRigidBody_ApplyLinearImpulse(void * instance, struct Vector3 impulse) {
	printf("invoke HkRigidBody_ApplyLinearImpulse\n");
	return __PVE_HkRigidBody_ApplyLinearImpulse(instance, impulse);
}

void (*__PVE_HkRigidBody_ApplyPointImpulse)(void * instance, struct Vector3 impulse, struct Vector3 point) __attribute__((ms_abi));
void HkRigidBody_ApplyPointImpulse(void * instance, struct Vector3 impulse, struct Vector3 point) {
	printf("invoke HkRigidBody_ApplyPointImpulse\n");
	return __PVE_HkRigidBody_ApplyPointImpulse(instance, impulse, point);
}

void (*__PVE_HkRigidBody_ApplyAngularImpulse)(void * instance, struct Vector3 impulse) __attribute__((ms_abi));
void HkRigidBody_ApplyAngularImpulse(void * instance, struct Vector3 impulse) {
	printf("invoke HkRigidBody_ApplyAngularImpulse\n");
	return __PVE_HkRigidBody_ApplyAngularImpulse(instance, impulse);
}

void (*__PVE_HkRigidBody_SetLayer)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBody_SetLayer(void * instance, int value) {
	printf("invoke HkRigidBody_SetLayer\n");
	return __PVE_HkRigidBody_SetLayer(instance, value);
}

int (*__PVE_HkRigidBody_GetCollisionFilterInfo)(void * instance) __attribute__((ms_abi));
int HkRigidBody_GetCollisionFilterInfo(void * instance) {
	printf("invoke HkRigidBody_GetCollisionFilterInfo\n");
	return __PVE_HkRigidBody_GetCollisionFilterInfo(instance);
}

void (*__PVE_HkRigidBody_SetCollisionFilterInfo)(void * instance, int info) __attribute__((ms_abi));
void HkRigidBody_SetCollisionFilterInfo(void * instance, int info) {
	printf("invoke HkRigidBody_SetCollisionFilterInfo\n");
	return __PVE_HkRigidBody_SetCollisionFilterInfo(instance, info);
}

void (*__PVE_HkRigidBody_ApplyForce)(void * instance, float time, struct Vector3 force) __attribute__((ms_abi));
void HkRigidBody_ApplyForce(void * instance, float time, struct Vector3 force) {
	printf("invoke HkRigidBody_ApplyForce\n");
	return __PVE_HkRigidBody_ApplyForce(instance, time, force);
}

void (*__PVE_HkRigidBody_ApplyForceToPoint)(void * instance, float time, struct Vector3 force, struct Vector3 point) __attribute__((ms_abi));
void HkRigidBody_ApplyForceToPoint(void * instance, float time, struct Vector3 force, struct Vector3 point) {
	printf("invoke HkRigidBody_ApplyForceToPoint\n");
	return __PVE_HkRigidBody_ApplyForceToPoint(instance, time, force, point);
}

void (*__PVE_HkRigidBody_ApplyTorque)(void * instance, float time, struct Vector3 torque) __attribute__((ms_abi));
void HkRigidBody_ApplyTorque(void * instance, float time, struct Vector3 torque) {
	printf("invoke HkRigidBody_ApplyTorque\n");
	return __PVE_HkRigidBody_ApplyTorque(instance, time, torque);
}

void * (*__PVE_HkRigidBody_GetNativeObjectName)(void * instance) __attribute__((ms_abi));
void * HkRigidBody_GetNativeObjectName(void * instance) {
	printf("invoke HkRigidBody_GetNativeObjectName\n");
	return __PVE_HkRigidBody_GetNativeObjectName(instance);
}

void (*__PVE_HkRigidBody_RemoveFromWorld)(void * instance) __attribute__((ms_abi));
void HkRigidBody_RemoveFromWorld(void * instance) {
	printf("invoke HkRigidBody_RemoveFromWorld\n");
	return __PVE_HkRigidBody_RemoveFromWorld(instance);
}

int (*__PVE_HkRigidBody_HasGravity)(void * instance) __attribute__((ms_abi));
int HkRigidBody_HasGravity(void * instance) {
	printf("invoke HkRigidBody_HasGravity\n");
	return __PVE_HkRigidBody_HasGravity(instance);
}

int (*__PVE_HkRigidBody_HasConstraints)(void * instance) __attribute__((ms_abi));
int HkRigidBody_HasConstraints(void * instance) {
	printf("invoke HkRigidBody_HasConstraints\n");
	return __PVE_HkRigidBody_HasConstraints(instance);
}

void * (*__PVE_HkRigidBody_GetBreakableBody)(void * instance) __attribute__((ms_abi));
void * HkRigidBody_GetBreakableBody(void * instance) {
	printf("invoke HkRigidBody_GetBreakableBody\n");
	return __PVE_HkRigidBody_GetBreakableBody(instance);
}

struct Vector3 (*__PVE_HkRigidBody_GetGravity)(void * gravityAction) __attribute__((ms_abi));
struct Vector3 HkRigidBody_GetGravity(void * gravityAction) {
	printf("invoke HkRigidBody_GetGravity\n");
	return __PVE_HkRigidBody_GetGravity(gravityAction);
}

void (*__PVE_HkRigidBody_ReleaseGravity)(void * gravityAction) __attribute__((ms_abi));
void HkRigidBody_ReleaseGravity(void * gravityAction) {
	printf("invoke HkRigidBody_ReleaseGravity\n");
	return __PVE_HkRigidBody_ReleaseGravity(gravityAction);
}

void (*__PVE_HkRigidBody_SetGravity)(void * gravityAction, struct Vector3 gravity) __attribute__((ms_abi));
void HkRigidBody_SetGravity(void * gravityAction, struct Vector3 gravity) {
	printf("invoke HkRigidBody_SetGravity\n");
	return __PVE_HkRigidBody_SetGravity(gravityAction, gravity);
}

void * (*__PVE_HkRigidBody_Clone)(void * cloneBody) __attribute__((ms_abi));
void * HkRigidBody_Clone(void * cloneBody) {
	printf("invoke HkRigidBody_Clone\n");
	return __PVE_HkRigidBody_Clone(cloneBody);
}

void * (*__PVE_HkRigidBody_FromShape)(void * shape) __attribute__((ms_abi));
void * HkRigidBody_FromShape(void * shape) {
	printf("invoke HkRigidBody_FromShape\n");
	return __PVE_HkRigidBody_FromShape(shape);
}

long int (*__PVE_HkRigidBody_GetGcRoot)(void * instance) __attribute__((ms_abi));
long int HkRigidBody_GetGcRoot(void * instance) {
	printf("invoke HkRigidBody_GetGcRoot\n");
	return __PVE_HkRigidBody_GetGcRoot(instance);
}

void * (*__PVE_HkRigidBody_GetGravityAction)(void * instance, void * action, struct Vector3 gravity) __attribute__((ms_abi));
void * HkRigidBody_GetGravityAction(void * instance, void * action, struct Vector3 gravity) {
	printf("invoke HkRigidBody_GetGravityAction\n");
	return __PVE_HkRigidBody_GetGravityAction(instance, action, gravity);
}

void (*__PVE_HkRigidBody_AddGravityAction)(void * instance, void * action) __attribute__((ms_abi));
void HkRigidBody_AddGravityAction(void * instance, void * action) {
	printf("invoke HkRigidBody_AddGravityAction\n");
	return __PVE_HkRigidBody_AddGravityAction(instance, action);
}

int (*__PVE_HkRigidBody_GetDeactivationCounter0)(void * instance) __attribute__((ms_abi));
int HkRigidBody_GetDeactivationCounter0(void * instance) {
	printf("invoke HkRigidBody_GetDeactivationCounter0\n");
	return __PVE_HkRigidBody_GetDeactivationCounter0(instance);
}

int (*__PVE_HkRigidBody_GetDeactivationCounter1)(void * instance) __attribute__((ms_abi));
int HkRigidBody_GetDeactivationCounter1(void * instance) {
	printf("invoke HkRigidBody_GetDeactivationCounter1\n");
	return __PVE_HkRigidBody_GetDeactivationCounter1(instance);
}

int (*__PVE_HkRigidBody_HasActions)(void * instance, int actionType) __attribute__((ms_abi));
int HkRigidBody_HasActions(void * instance, int actionType) {
	printf("invoke HkRigidBody_HasActions\n");
	return __PVE_HkRigidBody_HasActions(instance, actionType);
}

void * (*__PVE_HkRigidBodyCinfo_Create)() __attribute__((ms_abi));
void * HkRigidBodyCinfo_Create() {
	printf("invoke HkRigidBodyCinfo_Create\n");
	return __PVE_HkRigidBodyCinfo_Create();
}

void (*__PVE_HkRigidBodyCinfo_Release)(void * instance) __attribute__((ms_abi));
void HkRigidBodyCinfo_Release(void * instance) {
	printf("invoke HkRigidBodyCinfo_Release\n");
	return __PVE_HkRigidBodyCinfo_Release(instance);
}

int (*__PVE_HkRigidBodyCinfo_GetCollisionResponse)(void * instance) __attribute__((ms_abi));
int HkRigidBodyCinfo_GetCollisionResponse(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetCollisionResponse\n");
	return __PVE_HkRigidBodyCinfo_GetCollisionResponse(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetCollisionResponse)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetCollisionResponse(void * instance, int value) {
	printf("invoke HkRigidBodyCinfo_SetCollisionResponse\n");
	return __PVE_HkRigidBodyCinfo_SetCollisionResponse(instance, value);
}

int (*__PVE_HkRigidBodyCinfo_GetResponseModifiers)(void * instance) __attribute__((ms_abi));
int HkRigidBodyCinfo_GetResponseModifiers(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetResponseModifiers\n");
	return __PVE_HkRigidBodyCinfo_GetResponseModifiers(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetResponseModifiers)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetResponseModifiers(void * instance, int value) {
	printf("invoke HkRigidBodyCinfo_SetResponseModifiers\n");
	return __PVE_HkRigidBodyCinfo_SetResponseModifiers(instance, value);
}

struct Vector3 (*__PVE_HkRigidBodyCinfo_GetPosition)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBodyCinfo_GetPosition(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetPosition\n");
	return __PVE_HkRigidBodyCinfo_GetPosition(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetPosition)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetPosition(void * instance, struct Vector3 value) {
	printf("invoke HkRigidBodyCinfo_SetPosition\n");
	return __PVE_HkRigidBodyCinfo_SetPosition(instance, value);
}

struct Quaternion (*__PVE_HkRigidBodyCinfo_GetRotation)(void * instance) __attribute__((ms_abi));
struct Quaternion HkRigidBodyCinfo_GetRotation(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetRotation\n");
	return __PVE_HkRigidBodyCinfo_GetRotation(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetRotation)(void * instance, struct Quaternion value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetRotation(void * instance, struct Quaternion value) {
	printf("invoke HkRigidBodyCinfo_SetRotation\n");
	return __PVE_HkRigidBodyCinfo_SetRotation(instance, value);
}

struct Vector3 (*__PVE_HkRigidBodyCinfo_GetLinearVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBodyCinfo_GetLinearVelocity(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetLinearVelocity\n");
	return __PVE_HkRigidBodyCinfo_GetLinearVelocity(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetLinearVelocity)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetLinearVelocity(void * instance, struct Vector3 value) {
	printf("invoke HkRigidBodyCinfo_SetLinearVelocity\n");
	return __PVE_HkRigidBodyCinfo_SetLinearVelocity(instance, value);
}

struct Vector3 (*__PVE_HkRigidBodyCinfo_GetAngularVelocity)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBodyCinfo_GetAngularVelocity(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetAngularVelocity\n");
	return __PVE_HkRigidBodyCinfo_GetAngularVelocity(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetAngularVelocity)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetAngularVelocity(void * instance, struct Vector3 value) {
	printf("invoke HkRigidBodyCinfo_SetAngularVelocity\n");
	return __PVE_HkRigidBodyCinfo_SetAngularVelocity(instance, value);
}

struct Vector3 (*__PVE_HkRigidBodyCinfo_GetCenterOfMass)(void * instance) __attribute__((ms_abi));
struct Vector3 HkRigidBodyCinfo_GetCenterOfMass(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetCenterOfMass\n");
	return __PVE_HkRigidBodyCinfo_GetCenterOfMass(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetCenterOfMass)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetCenterOfMass(void * instance, struct Vector3 value) {
	printf("invoke HkRigidBodyCinfo_SetCenterOfMass\n");
	return __PVE_HkRigidBodyCinfo_SetCenterOfMass(instance, value);
}

float (*__PVE_HkRigidBodyCinfo_GetMass)(void * instance) __attribute__((ms_abi));
float HkRigidBodyCinfo_GetMass(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetMass\n");
	return __PVE_HkRigidBodyCinfo_GetMass(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetMass)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetMass(void * instance, float value) {
	printf("invoke HkRigidBodyCinfo_SetMass\n");
	return __PVE_HkRigidBodyCinfo_SetMass(instance, value);
}

float (*__PVE_HkRigidBodyCinfo_GetLinearDamping)(void * instance) __attribute__((ms_abi));
float HkRigidBodyCinfo_GetLinearDamping(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetLinearDamping\n");
	return __PVE_HkRigidBodyCinfo_GetLinearDamping(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetLinearDamping)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetLinearDamping(void * instance, float value) {
	printf("invoke HkRigidBodyCinfo_SetLinearDamping\n");
	return __PVE_HkRigidBodyCinfo_SetLinearDamping(instance, value);
}

float (*__PVE_HkRigidBodyCinfo_GetAngularDamping)(void * instance) __attribute__((ms_abi));
float HkRigidBodyCinfo_GetAngularDamping(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetAngularDamping\n");
	return __PVE_HkRigidBodyCinfo_GetAngularDamping(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetAngularDamping)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetAngularDamping(void * instance, float value) {
	printf("invoke HkRigidBodyCinfo_SetAngularDamping\n");
	return __PVE_HkRigidBodyCinfo_SetAngularDamping(instance, value);
}

float (*__PVE_HkRigidBodyCinfo_GetFriction)(void * instance) __attribute__((ms_abi));
float HkRigidBodyCinfo_GetFriction(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetFriction\n");
	return __PVE_HkRigidBodyCinfo_GetFriction(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetFriction)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetFriction(void * instance, float value) {
	printf("invoke HkRigidBodyCinfo_SetFriction\n");
	return __PVE_HkRigidBodyCinfo_SetFriction(instance, value);
}

float (*__PVE_HkRigidBodyCinfo_GetRestitution)(void * instance) __attribute__((ms_abi));
float HkRigidBodyCinfo_GetRestitution(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetRestitution\n");
	return __PVE_HkRigidBodyCinfo_GetRestitution(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetRestitution)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetRestitution(void * instance, float value) {
	printf("invoke HkRigidBodyCinfo_SetRestitution\n");
	return __PVE_HkRigidBodyCinfo_SetRestitution(instance, value);
}

float (*__PVE_HkRigidBodyCinfo_GetMaxLinearVelocity)(void * instance) __attribute__((ms_abi));
float HkRigidBodyCinfo_GetMaxLinearVelocity(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetMaxLinearVelocity\n");
	return __PVE_HkRigidBodyCinfo_GetMaxLinearVelocity(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetMaxLinearVelocity)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetMaxLinearVelocity(void * instance, float value) {
	printf("invoke HkRigidBodyCinfo_SetMaxLinearVelocity\n");
	return __PVE_HkRigidBodyCinfo_SetMaxLinearVelocity(instance, value);
}

float (*__PVE_HkRigidBodyCinfo_GetMaxAngularVelocity)(void * instance) __attribute__((ms_abi));
float HkRigidBodyCinfo_GetMaxAngularVelocity(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetMaxAngularVelocity\n");
	return __PVE_HkRigidBodyCinfo_GetMaxAngularVelocity(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetMaxAngularVelocity)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetMaxAngularVelocity(void * instance, float value) {
	printf("invoke HkRigidBodyCinfo_SetMaxAngularVelocity\n");
	return __PVE_HkRigidBodyCinfo_SetMaxAngularVelocity(instance, value);
}

short (*__PVE_HkRigidBodyCinfo_GetContactPointCallbackDelay)(void * instance) __attribute__((ms_abi));
short HkRigidBodyCinfo_GetContactPointCallbackDelay(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetContactPointCallbackDelay\n");
	return __PVE_HkRigidBodyCinfo_GetContactPointCallbackDelay(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetContactPointCallbackDelay)(void * instance, short value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetContactPointCallbackDelay(void * instance, short value) {
	printf("invoke HkRigidBodyCinfo_SetContactPointCallbackDelay\n");
	return __PVE_HkRigidBodyCinfo_SetContactPointCallbackDelay(instance, value);
}

float (*__PVE_HkRigidBodyCinfo_GetAllowedPenetrationDepth)(void * instance) __attribute__((ms_abi));
float HkRigidBodyCinfo_GetAllowedPenetrationDepth(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetAllowedPenetrationDepth\n");
	return __PVE_HkRigidBodyCinfo_GetAllowedPenetrationDepth(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetAllowedPenetrationDepth)(void * instance, float value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetAllowedPenetrationDepth(void * instance, float value) {
	printf("invoke HkRigidBodyCinfo_SetAllowedPenetrationDepth\n");
	return __PVE_HkRigidBodyCinfo_SetAllowedPenetrationDepth(instance, value);
}

int (*__PVE_HkRigidBodyCinfo_GetMotionType)(void * instance) __attribute__((ms_abi));
int HkRigidBodyCinfo_GetMotionType(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetMotionType\n");
	return __PVE_HkRigidBodyCinfo_GetMotionType(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetMotionType)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetMotionType(void * instance, int value) {
	printf("invoke HkRigidBodyCinfo_SetMotionType\n");
	return __PVE_HkRigidBodyCinfo_SetMotionType(instance, value);
}

int (*__PVE_HkRigidBodyCinfo_GetSolverDeactivation)(void * instance) __attribute__((ms_abi));
int HkRigidBodyCinfo_GetSolverDeactivation(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetSolverDeactivation\n");
	return __PVE_HkRigidBodyCinfo_GetSolverDeactivation(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetSolverDeactivation)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetSolverDeactivation(void * instance, int value) {
	printf("invoke HkRigidBodyCinfo_SetSolverDeactivation\n");
	return __PVE_HkRigidBodyCinfo_SetSolverDeactivation(instance, value);
}

int (*__PVE_HkRigidBodyCinfo_GetQualityType)(void * instance) __attribute__((ms_abi));
int HkRigidBodyCinfo_GetQualityType(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetQualityType\n");
	return __PVE_HkRigidBodyCinfo_GetQualityType(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetQualityType)(void * instance, int value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetQualityType(void * instance, int value) {
	printf("invoke HkRigidBodyCinfo_SetQualityType\n");
	return __PVE_HkRigidBodyCinfo_SetQualityType(instance, value);
}

char (*__PVE_HkRigidBodyCinfo_GetAutoRemoveLevel)(void * instance) __attribute__((ms_abi));
char HkRigidBodyCinfo_GetAutoRemoveLevel(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetAutoRemoveLevel\n");
	return __PVE_HkRigidBodyCinfo_GetAutoRemoveLevel(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetAutoRemoveLevel)(void * instance, char value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetAutoRemoveLevel(void * instance, char value) {
	printf("invoke HkRigidBodyCinfo_SetAutoRemoveLevel\n");
	return __PVE_HkRigidBodyCinfo_SetAutoRemoveLevel(instance, value);
}

void * (*__PVE_HkRigidBodyCinfo_GetShape)(void * instance) __attribute__((ms_abi));
void * HkRigidBodyCinfo_GetShape(void * instance) {
	printf("invoke HkRigidBodyCinfo_GetShape\n");
	return __PVE_HkRigidBodyCinfo_GetShape(instance);
}

void (*__PVE_HkRigidBodyCinfo_SetShape)(void * instance, void * value) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetShape(void * instance, void * value) {
	printf("invoke HkRigidBodyCinfo_SetShape\n");
	return __PVE_HkRigidBodyCinfo_SetShape(instance, value);
}

void (*__PVE_HkRigidBodyCinfo_CalculateBoxInertiaTensor)(void * instance, struct Vector3 halfExtents, float mass) __attribute__((ms_abi));
void HkRigidBodyCinfo_CalculateBoxInertiaTensor(void * instance, struct Vector3 halfExtents, float mass) {
	printf("invoke HkRigidBodyCinfo_CalculateBoxInertiaTensor\n");
	return __PVE_HkRigidBodyCinfo_CalculateBoxInertiaTensor(instance, halfExtents, mass);
}

void (*__PVE_HkRigidBodyCinfo_CalculateSphereInertiaTensor)(void * instance, float radius, float mass) __attribute__((ms_abi));
void HkRigidBodyCinfo_CalculateSphereInertiaTensor(void * instance, float radius, float mass) {
	printf("invoke HkRigidBodyCinfo_CalculateSphereInertiaTensor\n");
	return __PVE_HkRigidBodyCinfo_CalculateSphereInertiaTensor(instance, radius, mass);
}

void (*__PVE_HkRigidBodyCinfo_SetMassProperties)(void * instance, struct HkMassProperties properties) __attribute__((ms_abi));
void HkRigidBodyCinfo_SetMassProperties(void * instance, struct HkMassProperties properties) {
	printf("invoke HkRigidBodyCinfo_SetMassProperties\n");
	return __PVE_HkRigidBodyCinfo_SetMassProperties(instance, properties);
}

void (*__PVE_HkRigidBodyCinfo_ComputeShapeMass)(void * instance, void * shape, float mass) __attribute__((ms_abi));
void HkRigidBodyCinfo_ComputeShapeMass(void * instance, void * shape, float mass) {
	printf("invoke HkRigidBodyCinfo_ComputeShapeMass\n");
	return __PVE_HkRigidBodyCinfo_ComputeShapeMass(instance, shape, mass);
}

int (*__PVE_HkSimulationIsland_GetEntityCount)(void * island) __attribute__((ms_abi));
int HkSimulationIsland_GetEntityCount(void * island) {
	printf("invoke HkSimulationIsland_GetEntityCount\n");
	return __PVE_HkSimulationIsland_GetEntityCount(island);
}

void * (*__PVE_HkSimulationIsland_GetEntity)(void * island, int index) __attribute__((ms_abi));
void * HkSimulationIsland_GetEntity(void * island, int index) {
	printf("invoke HkSimulationIsland_GetEntity\n");
	return __PVE_HkSimulationIsland_GetEntity(island, index);
}

void (*__PVE_HkSimulationIsland_GetBounds)(void * island, void * bb) __attribute__((ms_abi));
void HkSimulationIsland_GetBounds(void * island, void * bb) {
	printf("invoke HkSimulationIsland_GetBounds\n");
	return __PVE_HkSimulationIsland_GetBounds(island, bb);
}

void (*__PVE_HkSimulationIsland_GetOffsets)(void * activeOffset, void * activeBitFieldOffset) __attribute__((ms_abi));
void HkSimulationIsland_GetOffsets(void * activeOffset, void * activeBitFieldOffset) {
	printf("invoke HkSimulationIsland_GetOffsets\n");
	return __PVE_HkSimulationIsland_GetOffsets(activeOffset, activeBitFieldOffset);
}

void (*__PVE_HkTaskProfiler_Init)(void * onTaskStarted, void * onTaskFinished) __attribute__((ms_abi));
void HkTaskProfiler_Init(void * onTaskStarted, void * onTaskFinished) {
	printf("invoke HkTaskProfiler_Init\n");
	return __PVE_HkTaskProfiler_Init(_PVE_Trampoline_Havok_HkTaskProfiler_TaskStartedFuncCpp(onTaskStarted), _PVE_Trampoline_Havok_HkTaskProfiler_TaskFinishedFunc(onTaskFinished));
}

void (*__PVE_HkTaskProfiler_ReleaseResources)() __attribute__((ms_abi));
void HkTaskProfiler_ReleaseResources() {
	printf("invoke HkTaskProfiler_ReleaseResources\n");
	return __PVE_HkTaskProfiler_ReleaseResources();
}

void (*__PVE_HkTaskProfiler_HookJobQueue)(void * jobQueue) __attribute__((ms_abi));
void HkTaskProfiler_HookJobQueue(void * jobQueue) {
	printf("invoke HkTaskProfiler_HookJobQueue\n");
	return __PVE_HkTaskProfiler_HookJobQueue(jobQueue);
}

void (*__PVE_HkTaskProfiler_ReplayTimers)(void * blockBegin, void * blockEnd) __attribute__((ms_abi));
void HkTaskProfiler_ReplayTimers(void * blockBegin, void * blockEnd) {
	printf("invoke HkTaskProfiler_ReplayTimers\n");
	return __PVE_HkTaskProfiler_ReplayTimers(_PVE_Trampoline_Havok_HkTaskProfiler_BlockBeginFuncCpp(blockBegin), _PVE_Trampoline_Havok_HkTaskProfiler_BlockEndFunc(blockEnd));
}

void (*__PVE_HkTaskProfiler_Begin1)() __attribute__((ms_abi));
void HkTaskProfiler_Begin1() {
	printf("invoke HkTaskProfiler_Begin1\n");
	return __PVE_HkTaskProfiler_Begin1();
}

void (*__PVE_HkTaskProfiler_Begin2)() __attribute__((ms_abi));
void HkTaskProfiler_Begin2() {
	printf("invoke HkTaskProfiler_Begin2\n");
	return __PVE_HkTaskProfiler_Begin2();
}

void (*__PVE_HkTaskProfiler_Begin3)() __attribute__((ms_abi));
void HkTaskProfiler_Begin3() {
	printf("invoke HkTaskProfiler_Begin3\n");
	return __PVE_HkTaskProfiler_Begin3();
}

void (*__PVE_HkTaskProfiler_Begin4)() __attribute__((ms_abi));
void HkTaskProfiler_Begin4() {
	printf("invoke HkTaskProfiler_Begin4\n");
	return __PVE_HkTaskProfiler_Begin4();
}

void (*__PVE_HkTaskProfiler_Begin5)() __attribute__((ms_abi));
void HkTaskProfiler_Begin5() {
	printf("invoke HkTaskProfiler_Begin5\n");
	return __PVE_HkTaskProfiler_Begin5();
}

void (*__PVE_HkTaskProfiler_End)() __attribute__((ms_abi));
void HkTaskProfiler_End() {
	printf("invoke HkTaskProfiler_End\n");
	return __PVE_HkTaskProfiler_End();
}

void (*__PVE_HkVDB_SyncTimers)(void * threadPool) __attribute__((ms_abi));
void HkVDB_SyncTimers(void * threadPool) {
	printf("invoke HkVDB_SyncTimers\n");
	return __PVE_HkVDB_SyncTimers(threadPool);
}

void (*__PVE_HkVDB_StepVDB)(void * world, float timeInSec) __attribute__((ms_abi));
void HkVDB_StepVDB(void * world, float timeInSec) {
	printf("invoke HkVDB_StepVDB\n");
	return __PVE_HkVDB_StepVDB(world, timeInSec);
}

void (*__PVE_HkVDB_Start)() __attribute__((ms_abi));
void HkVDB_Start() {
	printf("invoke HkVDB_Start\n");
	return __PVE_HkVDB_Start();
}

void (*__PVE_HkVDB_ReleaseResources)() __attribute__((ms_abi));
void HkVDB_ReleaseResources() {
	printf("invoke HkVDB_ReleaseResources\n");
	return __PVE_HkVDB_ReleaseResources();
}

int (*__PVE_HkVDB_GetPort)() __attribute__((ms_abi));
int HkVDB_GetPort() {
	printf("invoke HkVDB_GetPort\n");
	return __PVE_HkVDB_GetPort();
}

void (*__PVE_HkVDB_SetPort)(int value) __attribute__((ms_abi));
void HkVDB_SetPort(int value) {
	printf("invoke HkVDB_SetPort\n");
	return __PVE_HkVDB_SetPort(value);
}

void (*__PVE_HkVDB_UpdateCamera)(void * from, void * to, void * up) __attribute__((ms_abi));
void HkVDB_UpdateCamera(void * from, void * to, void * up) {
	printf("invoke HkVDB_UpdateCamera\n");
	return __PVE_HkVDB_UpdateCamera(from, to, up);
}

void (*__PVE_HkVDB_Capture)(void * path) __attribute__((ms_abi));
void HkVDB_Capture(void * path) {
	printf("invoke HkVDB_Capture\n");
	return __PVE_HkVDB_Capture(path);
}

void (*__PVE_HkVDB_EndCapture)() __attribute__((ms_abi));
void HkVDB_EndCapture() {
	printf("invoke HkVDB_EndCapture\n");
	return __PVE_HkVDB_EndCapture();
}

void * (*__PVE_HkWorld_Create)(int enableGlobalGravity, float broadphaseCubeSideLength, float contactRestingVelocity, int enableMultithreading, int solverIterations, void * broadPhaseCallback) __attribute__((ms_abi));
void * HkWorld_Create(int enableGlobalGravity, float broadphaseCubeSideLength, float contactRestingVelocity, int enableMultithreading, int solverIterations, void * broadPhaseCallback) {
	printf("invoke HkWorld_Create\n");
	return __PVE_HkWorld_Create(enableGlobalGravity, broadphaseCubeSideLength, contactRestingVelocity, enableMultithreading, solverIterations, _PVE_Trampoline_Havok_HkWorld_BroadPhaseExitCallback(broadPhaseCallback));
}

void * (*__PVE_HkWorld_CreateCInfo)(void * cInfo, void * broadPhaseCallback) __attribute__((ms_abi));
void * HkWorld_CreateCInfo(void * cInfo, void * broadPhaseCallback) {
	printf("invoke HkWorld_CreateCInfo\n");
	return __PVE_HkWorld_CreateCInfo(cInfo, _PVE_Trampoline_Havok_HkWorld_BroadPhaseExitCallback(broadPhaseCallback));
}

void * (*__PVE_HkWorld_CreateBodyPairCollection)() __attribute__((ms_abi));
void * HkWorld_CreateBodyPairCollection() {
	printf("invoke HkWorld_CreateBodyPairCollection\n");
	return __PVE_HkWorld_CreateBodyPairCollection();
}

void (*__PVE_HkWorld_RegisterWithJobQueue)(void * world, void * jobQueue) __attribute__((ms_abi));
void HkWorld_RegisterWithJobQueue(void * world, void * jobQueue) {
	printf("invoke HkWorld_RegisterWithJobQueue\n");
	return __PVE_HkWorld_RegisterWithJobQueue(world, jobQueue);
}

void (*__PVE_HkWorld_Lock)(void * world) __attribute__((ms_abi));
void HkWorld_Lock(void * world) {
	printf("invoke HkWorld_Lock\n");
	return __PVE_HkWorld_Lock(world);
}

void (*__PVE_HkWorld_Unlock)(void * world) __attribute__((ms_abi));
void HkWorld_Unlock(void * world) {
	printf("invoke HkWorld_Unlock\n");
	return __PVE_HkWorld_Unlock(world);
}

void (*__PVE_HkWorld_LockCriticalOperations)(void * world) __attribute__((ms_abi));
void HkWorld_LockCriticalOperations(void * world) {
	printf("invoke HkWorld_LockCriticalOperations\n");
	return __PVE_HkWorld_LockCriticalOperations(world);
}

void (*__PVE_HkWorld_UnlockCriticalOperations)(void * world) __attribute__((ms_abi));
void HkWorld_UnlockCriticalOperations(void * world) {
	printf("invoke HkWorld_UnlockCriticalOperations\n");
	return __PVE_HkWorld_UnlockCriticalOperations(world);
}

void (*__PVE_HkWorld_ExecutePendingCriticalOperations)(void * world) __attribute__((ms_abi));
void HkWorld_ExecutePendingCriticalOperations(void * world) {
	printf("invoke HkWorld_ExecutePendingCriticalOperations\n");
	return __PVE_HkWorld_ExecutePendingCriticalOperations(world);
}

void (*__PVE_HkWorld_StepDeltaTime)(void * world, float deltaTime) __attribute__((ms_abi));
void HkWorld_StepDeltaTime(void * world, float deltaTime) {
	printf("invoke HkWorld_StepDeltaTime\n");
	return __PVE_HkWorld_StepDeltaTime(world, deltaTime);
}

void (*__PVE_HkWorld_StepMultiThreaded)(void * world, void * jobQueue, void * threadPool, float deltaTime) __attribute__((ms_abi));
void HkWorld_StepMultiThreaded(void * world, void * jobQueue, void * threadPool, float deltaTime) {
	printf("invoke HkWorld_StepMultiThreaded\n");
	return __PVE_HkWorld_StepMultiThreaded(world, jobQueue, threadPool, deltaTime);
}

int (*__PVE_HkWorld_InitMtStep)(void * world, void * jobQueue, float deltaTime) __attribute__((ms_abi));
int HkWorld_InitMtStep(void * world, void * jobQueue, float deltaTime) {
	printf("invoke HkWorld_InitMtStep\n");
	return __PVE_HkWorld_InitMtStep(world, jobQueue, deltaTime);
}

int (*__PVE_HkWorld_FinishMtStep)(void * world, void * jobQueue, void * threadPool) __attribute__((ms_abi));
int HkWorld_FinishMtStep(void * world, void * jobQueue, void * threadPool) {
	printf("invoke HkWorld_FinishMtStep\n");
	return __PVE_HkWorld_FinishMtStep(world, jobQueue, threadPool);
}

void (*__PVE_HkWorld_ExecuteViolatedConstraintProjections)(void * world, void * constraintListener, int doProjections) __attribute__((ms_abi));
void HkWorld_ExecuteViolatedConstraintProjections(void * world, void * constraintListener, int doProjections) {
	printf("invoke HkWorld_ExecuteViolatedConstraintProjections\n");
	return __PVE_HkWorld_ExecuteViolatedConstraintProjections(world, constraintListener, doProjections);
}

void (*__PVE_HkWorld_ReportRuntimeDataConstraints)(void * world) __attribute__((ms_abi));
void HkWorld_ReportRuntimeDataConstraints(void * world) {
	printf("invoke HkWorld_ReportRuntimeDataConstraints\n");
	return __PVE_HkWorld_ReportRuntimeDataConstraints(world);
}

void (*__PVE_HkWorld_AddConstraint)(void * world, void * constraint) __attribute__((ms_abi));
void HkWorld_AddConstraint(void * world, void * constraint) {
	printf("invoke HkWorld_AddConstraint\n");
	return __PVE_HkWorld_AddConstraint(world, constraint);
}

void (*__PVE_HkWorld_RemoveConstraint)(void * world, void * constraint) __attribute__((ms_abi));
void HkWorld_RemoveConstraint(void * world, void * constraint) {
	printf("invoke HkWorld_RemoveConstraint\n");
	return __PVE_HkWorld_RemoveConstraint(world, constraint);
}

void (*__PVE_HkWorld_AddEntity)(void * world, void * entity) __attribute__((ms_abi));
void HkWorld_AddEntity(void * world, void * entity) {
	printf("invoke HkWorld_AddEntity\n");
	return __PVE_HkWorld_AddEntity(world, entity);
}

void (*__PVE_HkWorld_RemoveEntity)(void * world, void * entity) __attribute__((ms_abi));
void HkWorld_RemoveEntity(void * world, void * entity) {
	printf("invoke HkWorld_RemoveEntity\n");
	return __PVE_HkWorld_RemoveEntity(world, entity);
}

void (*__PVE_HkWorld_AddPhantom)(void * world, void * phantom) __attribute__((ms_abi));
void HkWorld_AddPhantom(void * world, void * phantom) {
	printf("invoke HkWorld_AddPhantom\n");
	return __PVE_HkWorld_AddPhantom(world, phantom);
}

void (*__PVE_HkWorld_RemovePhantom)(void * world, void * phantom) __attribute__((ms_abi));
void HkWorld_RemovePhantom(void * world, void * phantom) {
	printf("invoke HkWorld_RemovePhantom\n");
	return __PVE_HkWorld_RemovePhantom(world, phantom);
}

void (*__PVE_HkWorld_AddPhysicsSystem)(void * world, void * system) __attribute__((ms_abi));
void HkWorld_AddPhysicsSystem(void * world, void * system) {
	printf("invoke HkWorld_AddPhysicsSystem\n");
	return __PVE_HkWorld_AddPhysicsSystem(world, system);
}

void (*__PVE_HkWorld_RemovePhysicsSystem)(void * world, void * system) __attribute__((ms_abi));
void HkWorld_RemovePhysicsSystem(void * world, void * system) {
	printf("invoke HkWorld_RemovePhysicsSystem\n");
	return __PVE_HkWorld_RemovePhysicsSystem(world, system);
}

void (*__PVE_HkWorld_GetPenetrationsShape)(void * world, void * bodyCollector, void * shape, struct Vector3 translation, struct Quaternion rotation, int filter, void * buffer) __attribute__((ms_abi));
void HkWorld_GetPenetrationsShape(void * world, void * bodyCollector, void * shape, struct Vector3 translation, struct Quaternion rotation, int filter, void * buffer) {
	printf("invoke HkWorld_GetPenetrationsShape\n");
	return __PVE_HkWorld_GetPenetrationsShape(world, bodyCollector, shape, translation, rotation, filter, buffer);
}

void (*__PVE_HkWorld_GetPenetrationsBox)(void * world, void * bodyCollector, struct Vector3 halfExtents, struct Vector3 translation, struct Quaternion rotation, int filter, void * buffer) __attribute__((ms_abi));
void HkWorld_GetPenetrationsBox(void * world, void * bodyCollector, struct Vector3 halfExtents, struct Vector3 translation, struct Quaternion rotation, int filter, void * buffer) {
	printf("invoke HkWorld_GetPenetrationsBox\n");
	return __PVE_HkWorld_GetPenetrationsBox(world, bodyCollector, halfExtents, translation, rotation, filter, buffer);
}

void (*__PVE_HkWorld_GetPenetrationsShapeShape)(void * world, void * bodyCollector, void * shape1, struct Vector3 translation1, struct Quaternion rotation1, void * shape2, struct Vector3 translation2, struct Quaternion rotation2, void * buffer) __attribute__((ms_abi));
void HkWorld_GetPenetrationsShapeShape(void * world, void * bodyCollector, void * shape1, struct Vector3 translation1, struct Quaternion rotation1, void * shape2, struct Vector3 translation2, struct Quaternion rotation2, void * buffer) {
	printf("invoke HkWorld_GetPenetrationsShapeShape\n");
	return __PVE_HkWorld_GetPenetrationsShapeShape(world, bodyCollector, shape1, translation1, rotation1, shape2, translation2, rotation2, buffer);
}

int (*__PVE_HkWorld_IsPenetratingShapeShape)(void * world, void * shape1, struct Vector3 translation1, struct Quaternion rotation1, void * shape2, struct Vector3 translation2, struct Quaternion rotation2) __attribute__((ms_abi));
int HkWorld_IsPenetratingShapeShape(void * world, void * shape1, struct Vector3 translation1, struct Quaternion rotation1, void * shape2, struct Vector3 translation2, struct Quaternion rotation2) {
	printf("invoke HkWorld_IsPenetratingShapeShape\n");
	return __PVE_HkWorld_IsPenetratingShapeShape(world, shape1, translation1, rotation1, shape2, translation2, rotation2);
}

int (*__PVE_HkWorld_IsPenetratingShapeShapeTransform)(void * world, void * shape1, struct Matrix transform1, void * shape2, struct Matrix transform2) __attribute__((ms_abi));
int HkWorld_IsPenetratingShapeShapeTransform(void * world, void * shape1, struct Matrix transform1, void * shape2, struct Matrix transform2) {
	printf("invoke HkWorld_IsPenetratingShapeShapeTransform\n");
	return __PVE_HkWorld_IsPenetratingShapeShapeTransform(world, shape1, transform1, shape2, transform2);
}

int (*__PVE_HkWorld_CastShape)(void * world, struct Vector3 to, void * shape, struct Matrix transform, int filterLayer, float extraPenetration, void * outResult) __attribute__((ms_abi));
int HkWorld_CastShape(void * world, struct Vector3 to, void * shape, struct Matrix transform, int filterLayer, float extraPenetration, void * outResult) {
	printf("invoke HkWorld_CastShape\n");
	return __PVE_HkWorld_CastShape(world, to, shape, transform, filterLayer, extraPenetration, outResult);
}

int (*__PVE_HkWorld_CastShapeReturnPoint)(void * world, struct Vector3 to, void * shape, struct Matrix transform, int filterLayer, float extraPenetration, void * outPosition) __attribute__((ms_abi));
int HkWorld_CastShapeReturnPoint(void * world, struct Vector3 to, void * shape, struct Matrix transform, int filterLayer, float extraPenetration, void * outPosition) {
	printf("invoke HkWorld_CastShapeReturnPoint\n");
	return __PVE_HkWorld_CastShapeReturnPoint(world, to, shape, transform, filterLayer, extraPenetration, outPosition);
}

int (*__PVE_HkWorld_CastShapeReturnContact)(void * world, struct Vector3 to, void * shape, struct Matrix transform, int filterLayer, float extraPenetration, void * outPoint) __attribute__((ms_abi));
int HkWorld_CastShapeReturnContact(void * world, struct Vector3 to, void * shape, struct Matrix transform, int filterLayer, float extraPenetration, void * outPoint) {
	printf("invoke HkWorld_CastShapeReturnContact\n");
	return __PVE_HkWorld_CastShapeReturnContact(world, to, shape, transform, filterLayer, extraPenetration, outPoint);
}

int (*__PVE_HkWorld_CastShapeReturnContactData)(void * world, struct Vector3 to, void * shape, struct Matrix transform, int collisionFilterInfo, float extraPenetration, void * outPosition, void * outNormal, void * outDistance) __attribute__((ms_abi));
int HkWorld_CastShapeReturnContactData(void * world, struct Vector3 to, void * shape, struct Matrix transform, int collisionFilterInfo, float extraPenetration, void * outPosition, void * outNormal, void * outDistance) {
	printf("invoke HkWorld_CastShapeReturnContactData\n");
	return __PVE_HkWorld_CastShapeReturnContactData(world, to, shape, transform, collisionFilterInfo, extraPenetration, outPosition, outNormal, outDistance);
}

int (*__PVE_HkWorld_CastShapeReturnContactBodyData)(void * world, struct Vector3 to, void * shape, struct Matrix transform, int collisionFilterInfo, float extraPenetration, void * hitInfo) __attribute__((ms_abi));
int HkWorld_CastShapeReturnContactBodyData(void * world, struct Vector3 to, void * shape, struct Matrix transform, int collisionFilterInfo, float extraPenetration, void * hitInfo) {
	printf("invoke HkWorld_CastShapeReturnContactBodyData\n");
	return __PVE_HkWorld_CastShapeReturnContactBodyData(world, to, shape, transform, collisionFilterInfo, extraPenetration, hitInfo);
}

int (*__PVE_HkWorld_CastShapeReturnContactBodyDatas)(void * world, struct Vector3 to, void * shape, struct Matrix transform, int collisionFilterInfo, float extraPenetration, void * buffer) __attribute__((ms_abi));
int HkWorld_CastShapeReturnContactBodyDatas(void * world, struct Vector3 to, void * shape, struct Matrix transform, int collisionFilterInfo, float extraPenetration, void * buffer) {
	printf("invoke HkWorld_CastShapeReturnContactBodyDatas\n");
	return __PVE_HkWorld_CastShapeReturnContactBodyDatas(world, to, shape, transform, collisionFilterInfo, extraPenetration, buffer);
}

void (*__PVE_HkWorld_CastRayAll)(void * world, struct Vector3 from, struct Vector3 to, int raycastFilterLayer, void * buffer) __attribute__((ms_abi));
void HkWorld_CastRayAll(void * world, struct Vector3 from, struct Vector3 to, int raycastFilterLayer, void * buffer) {
	printf("invoke HkWorld_CastRayAll\n");
	return __PVE_HkWorld_CastRayAll(world, from, to, raycastFilterLayer, buffer);
}

int (*__PVE_HkWorld_CastRayCollisionFilter)(void * world, struct Vector3 from, struct Vector3 to, int colllisionFilter, int ignoreConvexShape, void * outConvexRadius, void * hitInfo) __attribute__((ms_abi));
int HkWorld_CastRayCollisionFilter(void * world, struct Vector3 from, struct Vector3 to, int colllisionFilter, int ignoreConvexShape, void * outConvexRadius, void * hitInfo) {
	printf("invoke HkWorld_CastRayCollisionFilter\n");
	return __PVE_HkWorld_CastRayCollisionFilter(world, from, to, colllisionFilter, ignoreConvexShape, outConvexRadius, hitInfo);
}

int (*__PVE_HkWorld_CastRayFilterLayer)(void * world, struct Vector3 from, struct Vector3 to, int raycastFilterLayer, int useFilter, void * hitInfo) __attribute__((ms_abi));
int HkWorld_CastRayFilterLayer(void * world, struct Vector3 from, struct Vector3 to, int raycastFilterLayer, int useFilter, void * hitInfo) {
	printf("invoke HkWorld_CastRayFilterLayer\n");
	return __PVE_HkWorld_CastRayFilterLayer(world, from, to, raycastFilterLayer, useFilter, hitInfo);
}

void (*__PVE_HkWorld_MarkForWrite)(void * world) __attribute__((ms_abi));
void HkWorld_MarkForWrite(void * world) {
	printf("invoke HkWorld_MarkForWrite\n");
	return __PVE_HkWorld_MarkForWrite(world);
}

void (*__PVE_HkWorld_UnmarkForWrite)(void * world) __attribute__((ms_abi));
void HkWorld_UnmarkForWrite(void * world) {
	printf("invoke HkWorld_UnmarkForWrite\n");
	return __PVE_HkWorld_UnmarkForWrite(world);
}

void (*__PVE_HkWorld_RefreshCollisionFilterOnEntity)(void * world, void * entity) __attribute__((ms_abi));
void HkWorld_RefreshCollisionFilterOnEntity(void * world, void * entity) {
	printf("invoke HkWorld_RefreshCollisionFilterOnEntity\n");
	return __PVE_HkWorld_RefreshCollisionFilterOnEntity(world, entity);
}

void (*__PVE_HkWorld_RefreshCollisionFilterOnWorld)(void * world) __attribute__((ms_abi));
void HkWorld_RefreshCollisionFilterOnWorld(void * world) {
	printf("invoke HkWorld_RefreshCollisionFilterOnWorld\n");
	return __PVE_HkWorld_RefreshCollisionFilterOnWorld(world);
}

void (*__PVE_HkWorld_ReintegrateEntity)(void * world, void * entity) __attribute__((ms_abi));
void HkWorld_ReintegrateEntity(void * world, void * entity) {
	printf("invoke HkWorld_ReintegrateEntity\n");
	return __PVE_HkWorld_ReintegrateEntity(world, entity);
}

void (*__PVE_HkWorld_AddAction)(void * world, void * action) __attribute__((ms_abi));
void HkWorld_AddAction(void * world, void * action) {
	printf("invoke HkWorld_AddAction\n");
	return __PVE_HkWorld_AddAction(world, action);
}

void (*__PVE_HkWorld_RemoveAction)(void * world, void * action) __attribute__((ms_abi));
void HkWorld_RemoveAction(void * world, void * action) {
	printf("invoke HkWorld_RemoveAction\n");
	return __PVE_HkWorld_RemoveAction(world, action);
}

void * (*__PVE_HkWorld_EnsureBatchSizes)(void * arr, void * size, int count, int newCount) __attribute__((ms_abi));
void * HkWorld_EnsureBatchSizes(void * arr, void * size, int count, int newCount) {
	printf("invoke HkWorld_EnsureBatchSizes\n");
	return __PVE_HkWorld_EnsureBatchSizes(arr, size, count, newCount);
}

void (*__PVE_HkWorld_SetBatchBody)(void * arr, int index, void * body) __attribute__((ms_abi));
void HkWorld_SetBatchBody(void * arr, int index, void * body) {
	printf("invoke HkWorld_SetBatchBody\n");
	return __PVE_HkWorld_SetBatchBody(arr, index, body);
}

void (*__PVE_HkWorld_AddEntityBatch)(void * world, void * arr, int count) __attribute__((ms_abi));
void HkWorld_AddEntityBatch(void * world, void * arr, int count) {
	printf("invoke HkWorld_AddEntityBatch\n");
	return __PVE_HkWorld_AddEntityBatch(world, arr, count);
}

void (*__PVE_HkWorld_RemoveEntityBatch)(void * world, void * arr, int count) __attribute__((ms_abi));
void HkWorld_RemoveEntityBatch(void * world, void * arr, int count) {
	printf("invoke HkWorld_RemoveEntityBatch\n");
	return __PVE_HkWorld_RemoveEntityBatch(world, arr, count);
}

int (*__PVE_HkWorld_GetActiveSimulationIslandsCount)(void * world) __attribute__((ms_abi));
int HkWorld_GetActiveSimulationIslandsCount(void * world) {
	printf("invoke HkWorld_GetActiveSimulationIslandsCount\n");
	return __PVE_HkWorld_GetActiveSimulationIslandsCount(world);
}

int (*__PVE_HkWorld_GetActiveSimulationIslandEntities)(void * world, int islandIndex, void * entities) __attribute__((ms_abi));
int HkWorld_GetActiveSimulationIslandEntities(void * world, int islandIndex, void * entities) {
	printf("invoke HkWorld_GetActiveSimulationIslandEntities\n");
	return __PVE_HkWorld_GetActiveSimulationIslandEntities(world, islandIndex, entities);
}

void (*__PVE_HkWorld_DeactivateSimulationIslandRigidBodies)(void * world, void * rigidBody) __attribute__((ms_abi));
void HkWorld_DeactivateSimulationIslandRigidBodies(void * world, void * rigidBody) {
	printf("invoke HkWorld_DeactivateSimulationIslandRigidBodies\n");
	return __PVE_HkWorld_DeactivateSimulationIslandRigidBodies(world, rigidBody);
}

int (*__PVE_HkWorld_IsActiveSimulationIsland)(void * world, void * rigidBody) __attribute__((ms_abi));
int HkWorld_IsActiveSimulationIsland(void * world, void * rigidBody) {
	printf("invoke HkWorld_IsActiveSimulationIsland\n");
	return __PVE_HkWorld_IsActiveSimulationIsland(world, rigidBody);
}

int (*__PVE_HkWorld_GetConstraintCount)(void * world) __attribute__((ms_abi));
int HkWorld_GetConstraintCount(void * world) {
	printf("invoke HkWorld_GetConstraintCount\n");
	return __PVE_HkWorld_GetConstraintCount(world);
}

int (*__PVE_HkWorld_GetActionCount)(void * world) __attribute__((ms_abi));
int HkWorld_GetActionCount(void * world) {
	printf("invoke HkWorld_GetActionCount\n");
	return __PVE_HkWorld_GetActionCount(world);
}

void * (*__PVE_HkWorld_GetFixedBody)(void * world) __attribute__((ms_abi));
void * HkWorld_GetFixedBody(void * world) {
	printf("invoke HkWorld_GetFixedBody\n");
	return __PVE_HkWorld_GetFixedBody(world);
}

void (*__PVE_HkWorld_ReadSimulationIslandInfos)(void * world, void * buffer) __attribute__((ms_abi));
void HkWorld_ReadSimulationIslandInfos(void * world, void * buffer) {
	printf("invoke HkWorld_ReadSimulationIslandInfos\n");
	return __PVE_HkWorld_ReadSimulationIslandInfos(world, buffer);
}

struct Vector3 (*__PVE_HkWorld_GetGravity)(void * world) __attribute__((ms_abi));
struct Vector3 HkWorld_GetGravity(void * world) {
	printf("invoke HkWorld_GetGravity\n");
	return __PVE_HkWorld_GetGravity(world);
}

void (*__PVE_HkWorld_SetGravity)(void * world, struct Vector3 value) __attribute__((ms_abi));
void HkWorld_SetGravity(void * world, struct Vector3 value) {
	printf("invoke HkWorld_SetGravity\n");
	return __PVE_HkWorld_SetGravity(world, value);
}

float (*__PVE_HkWorld_GetDeactivationRotationSqrdA)(void * world) __attribute__((ms_abi));
float HkWorld_GetDeactivationRotationSqrdA(void * world) {
	printf("invoke HkWorld_GetDeactivationRotationSqrdA\n");
	return __PVE_HkWorld_GetDeactivationRotationSqrdA(world);
}

void (*__PVE_HkWorld_SetDeactivationRotationSqrdA)(void * world, float value) __attribute__((ms_abi));
void HkWorld_SetDeactivationRotationSqrdA(void * world, float value) {
	printf("invoke HkWorld_SetDeactivationRotationSqrdA\n");
	return __PVE_HkWorld_SetDeactivationRotationSqrdA(world, value);
}

float (*__PVE_HkWorld_GetDeactivationRotationSqrdB)(void * world) __attribute__((ms_abi));
float HkWorld_GetDeactivationRotationSqrdB(void * world) {
	printf("invoke HkWorld_GetDeactivationRotationSqrdB\n");
	return __PVE_HkWorld_GetDeactivationRotationSqrdB(world);
}

void (*__PVE_HkWorld_SetDeactivationRotationSqrdB)(void * world, float value) __attribute__((ms_abi));
void HkWorld_SetDeactivationRotationSqrdB(void * world, float value) {
	printf("invoke HkWorld_SetDeactivationRotationSqrdB\n");
	return __PVE_HkWorld_SetDeactivationRotationSqrdB(world, value);
}

void (*__PVE_HkWorld_AddWorldExtension)(void * world, void * extension) __attribute__((ms_abi));
void HkWorld_AddWorldExtension(void * world, void * extension) {
	printf("invoke HkWorld_AddWorldExtension\n");
	return __PVE_HkWorld_AddWorldExtension(world, extension);
}

void (*__PVE_HkWorld_Release)(void * world, void * filter, void * penetrationHits, void * addBatch, void * removeBatch) __attribute__((ms_abi));
void HkWorld_Release(void * world, void * filter, void * penetrationHits, void * addBatch, void * removeBatch) {
	printf("invoke HkWorld_Release\n");
	return __PVE_HkWorld_Release(world, filter, penetrationHits, addBatch, removeBatch);
}

void * (*__PVE_HkPhysicsContext_Create)() __attribute__((ms_abi));
void * HkPhysicsContext_Create() {
	printf("invoke HkPhysicsContext_Create\n");
	return __PVE_HkPhysicsContext_Create();
}

void (*__PVE_HkPhysicsContext_RegisterAllPhysicsProcesses)() __attribute__((ms_abi));
void HkPhysicsContext_RegisterAllPhysicsProcesses() {
	printf("invoke HkPhysicsContext_RegisterAllPhysicsProcesses\n");
	return __PVE_HkPhysicsContext_RegisterAllPhysicsProcesses();
}

void (*__PVE_HkPhysicsContext_AddWorld)(void * physicsContext, void * world) __attribute__((ms_abi));
void HkPhysicsContext_AddWorld(void * physicsContext, void * world) {
	printf("invoke HkPhysicsContext_AddWorld\n");
	return __PVE_HkPhysicsContext_AddWorld(physicsContext, world);
}

void (*__PVE_HkPhysicsContext_RemoveWorld)(void * physicsContext, void * world) __attribute__((ms_abi));
void HkPhysicsContext_RemoveWorld(void * physicsContext, void * world) {
	printf("invoke HkPhysicsContext_RemoveWorld\n");
	return __PVE_HkPhysicsContext_RemoveWorld(physicsContext, world);
}

int (*__PVE_HkPhysicsContext_GetNumWorlds)(void * physicsContext) __attribute__((ms_abi));
int HkPhysicsContext_GetNumWorlds(void * physicsContext) {
	printf("invoke HkPhysicsContext_GetNumWorlds\n");
	return __PVE_HkPhysicsContext_GetNumWorlds(physicsContext);
}

void (*__PVE_HkPhysicsContext_SyncTimers)(void * physicsContext, void * threadPool) __attribute__((ms_abi));
void HkPhysicsContext_SyncTimers(void * physicsContext, void * threadPool) {
	printf("invoke HkPhysicsContext_SyncTimers\n");
	return __PVE_HkPhysicsContext_SyncTimers(physicsContext, threadPool);
}

void (*__PVE_HkPhysicsContext_Release)(void * physicsContext) __attribute__((ms_abi));
void HkPhysicsContext_Release(void * physicsContext) {
	printf("invoke HkPhysicsContext_Release\n");
	return __PVE_HkPhysicsContext_Release(physicsContext);
}

void * (*__PVE_HkGroupFilter_Create)(void * world) __attribute__((ms_abi));
void * HkGroupFilter_Create(void * world) {
	printf("invoke HkGroupFilter_Create\n");
	return __PVE_HkGroupFilter_Create(world);
}

int (*__PVE_HkGroupFilter_IsCollisionEnabled)(void * filter, int colllinfo1, int collinfo2) __attribute__((ms_abi));
int HkGroupFilter_IsCollisionEnabled(void * filter, int colllinfo1, int collinfo2) {
	printf("invoke HkGroupFilter_IsCollisionEnabled\n");
	return __PVE_HkGroupFilter_IsCollisionEnabled(filter, colllinfo1, collinfo2);
}

void * (*__PVE_HkpAabbPhantom_Create)(struct Vector3 min, struct Vector3 max, int collisionFilterInfo, void * collidableAddedD, void * collidableRemovedD) __attribute__((ms_abi));
void * HkpAabbPhantom_Create(struct Vector3 min, struct Vector3 max, int collisionFilterInfo, void * collidableAddedD, void * collidableRemovedD) {
	printf("invoke HkpAabbPhantom_Create\n");
	return __PVE_HkpAabbPhantom_Create(min, max, collisionFilterInfo, _PVE_Trampoline_Havok_HkpAabbPhantom_CollidableAddedD(collidableAddedD), _PVE_Trampoline_Havok_HkpAabbPhantom_CollidableRemovedD(collidableRemovedD));
}

void (*__PVE_HkpAabbPhantom_GetAabb)(void * instance, void * min, void * max) __attribute__((ms_abi));
void HkpAabbPhantom_GetAabb(void * instance, void * min, void * max) {
	printf("invoke HkpAabbPhantom_GetAabb\n");
	return __PVE_HkpAabbPhantom_GetAabb(instance, min, max);
}

void (*__PVE_HkpAabbPhantom_SetAabb)(void * instance, struct Vector3 min, struct Vector3 max) __attribute__((ms_abi));
void HkpAabbPhantom_SetAabb(void * instance, struct Vector3 min, struct Vector3 max) {
	printf("invoke HkpAabbPhantom_SetAabb\n");
	return __PVE_HkpAabbPhantom_SetAabb(instance, min, max);
}

void (*__PVE_HkpAabbPhantom_Release)(void * instance) __attribute__((ms_abi));
void HkpAabbPhantom_Release(void * instance) {
	printf("invoke HkpAabbPhantom_Release\n");
	return __PVE_HkpAabbPhantom_Release(instance);
}

void * (*__PVE_HkpCollidableAddedEvent_GetRigidBody)(void * instance) __attribute__((ms_abi));
void * HkpCollidableAddedEvent_GetRigidBody(void * instance) {
	printf("invoke HkpCollidableAddedEvent_GetRigidBody\n");
	return __PVE_HkpCollidableAddedEvent_GetRigidBody(instance);
}

void * (*__PVE_HkpCollidableRemovedEvent_GetRigidBody)(void * instance) __attribute__((ms_abi));
void * HkpCollidableRemovedEvent_GetRigidBody(void * instance) {
	printf("invoke HkpCollidableRemovedEvent_GetRigidBody\n");
	return __PVE_HkpCollidableRemovedEvent_GetRigidBody(instance);
}

void (*__PVE_HkSimpleShapePhantom_SetTransform)(void * instance, struct Matrix matrix) __attribute__((ms_abi));
void HkSimpleShapePhantom_SetTransform(void * instance, struct Matrix matrix) {
	printf("invoke HkSimpleShapePhantom_SetTransform\n");
	return __PVE_HkSimpleShapePhantom_SetTransform(instance, matrix);
}

void * (*__PVE_HkSimpleShapePhantom_Create)(void * shape) __attribute__((ms_abi));
void * HkSimpleShapePhantom_Create(void * shape) {
	printf("invoke HkSimpleShapePhantom_Create\n");
	return __PVE_HkSimpleShapePhantom_Create(shape);
}

void * (*__PVE_HkSimpleShapePhantom_CreateWithLayer)(void * shape, int layer) __attribute__((ms_abi));
void * HkSimpleShapePhantom_CreateWithLayer(void * shape, int layer) {
	printf("invoke HkSimpleShapePhantom_CreateWithLayer\n");
	return __PVE_HkSimpleShapePhantom_CreateWithLayer(shape, layer);
}

void * (*__PVE_HkSimpleShapePhantom_GetShape)(void * instance) __attribute__((ms_abi));
void * HkSimpleShapePhantom_GetShape(void * instance) {
	printf("invoke HkSimpleShapePhantom_GetShape\n");
	return __PVE_HkSimpleShapePhantom_GetShape(instance);
}

int (*__PVE_HkPhysicsSystem_IsActive)(void * instance) __attribute__((ms_abi));
int HkPhysicsSystem_IsActive(void * instance) {
	printf("invoke HkPhysicsSystem_IsActive\n");
	return __PVE_HkPhysicsSystem_IsActive(instance);
}

void (*__PVE_HkPhysicsSystem_SetActive)(void * instance, int value) __attribute__((ms_abi));
void HkPhysicsSystem_SetActive(void * instance, int value) {
	printf("invoke HkPhysicsSystem_SetActive\n");
	return __PVE_HkPhysicsSystem_SetActive(instance, value);
}

void (*__PVE_HkPhysicsSystem_RecreateConstraints)(void * instance) __attribute__((ms_abi));
void HkPhysicsSystem_RecreateConstraints(void * instance) {
	printf("invoke HkPhysicsSystem_RecreateConstraints\n");
	return __PVE_HkPhysicsSystem_RecreateConstraints(instance);
}

void (*__PVE_HkPhysicsSystem_GetConstraintDataFromSystem)(void * instance, void * constraintBuffer) __attribute__((ms_abi));
void HkPhysicsSystem_GetConstraintDataFromSystem(void * instance, void * constraintBuffer) {
	printf("invoke HkPhysicsSystem_GetConstraintDataFromSystem\n");
	return __PVE_HkPhysicsSystem_GetConstraintDataFromSystem(instance, constraintBuffer);
}

void * (*__PVE_HkPhysicsSystem_GetName)(void * instance) __attribute__((ms_abi));
void * HkPhysicsSystem_GetName(void * instance) {
	printf("invoke HkPhysicsSystem_GetName\n");
	return __PVE_HkPhysicsSystem_GetName(instance);
}

void * (*__PVE_HkPhysicsSystem_LoadRagdollFromFile)(void * fileName) __attribute__((ms_abi));
void * HkPhysicsSystem_LoadRagdollFromFile(void * fileName) {
	printf("invoke HkPhysicsSystem_LoadRagdollFromFile\n");
	return __PVE_HkPhysicsSystem_LoadRagdollFromFile(fileName);
}

void * (*__PVE_HkPhysicsSystem_LoadRagdollFromBuffer)(void * buffer, int length) __attribute__((ms_abi));
void * HkPhysicsSystem_LoadRagdollFromBuffer(void * buffer, int length) {
	printf("invoke HkPhysicsSystem_LoadRagdollFromBuffer\n");
	return __PVE_HkPhysicsSystem_LoadRagdollFromBuffer(buffer, length);
}

int (*__PVE_HkPhysicsSystem_InitFromData)(void * loadedData, void * physicsSystem, void * bodyBuffer) __attribute__((ms_abi));
int HkPhysicsSystem_InitFromData(void * loadedData, void * physicsSystem, void * bodyBuffer) {
	printf("invoke HkPhysicsSystem_InitFromData\n");
	return __PVE_HkPhysicsSystem_InitFromData(loadedData, physicsSystem, bodyBuffer);
}

int (*__PVE_HkpGroupFilter_CalcFilterInfo)(int layer, int systemGroup, int subSystemId, int subSystemDontCollideWith) __attribute__((ms_abi));
int HkpGroupFilter_CalcFilterInfo(int layer, int systemGroup, int subSystemId, int subSystemDontCollideWith) {
	printf("invoke HkpGroupFilter_CalcFilterInfo\n");
	return __PVE_HkpGroupFilter_CalcFilterInfo(layer, systemGroup, subSystemId, subSystemDontCollideWith);
}

int (*__PVE_HkpGroupFilter_CalcFilterInfoFromCurrent)(int currentInfo, int collisionLayer) __attribute__((ms_abi));
int HkpGroupFilter_CalcFilterInfoFromCurrent(int currentInfo, int collisionLayer) {
	printf("invoke HkpGroupFilter_CalcFilterInfoFromCurrent\n");
	return __PVE_HkpGroupFilter_CalcFilterInfoFromCurrent(currentInfo, collisionLayer);
}

void (*__PVE_HkpInertiaTensorComputer_OptimizeInertiasOfConstraintTree)(void * constraints, int size, void * rigidBody) __attribute__((ms_abi));
void HkpInertiaTensorComputer_OptimizeInertiasOfConstraintTree(void * constraints, int size, void * rigidBody) {
	printf("invoke HkpInertiaTensorComputer_OptimizeInertiasOfConstraintTree\n");
	return __PVE_HkpInertiaTensorComputer_OptimizeInertiasOfConstraintTree(constraints, size, rigidBody);
}

void (*__PVE_HkPhysicsSystem_Release)(void * physicsSystem) __attribute__((ms_abi));
void HkPhysicsSystem_Release(void * physicsSystem) {
	printf("invoke HkPhysicsSystem_Release\n");
	return __PVE_HkPhysicsSystem_Release(physicsSystem);
}

float (*__PVE_HkRagdollConstraintData_GetPlaneMinAngularLimit)(void * instance) __attribute__((ms_abi));
float HkRagdollConstraintData_GetPlaneMinAngularLimit(void * instance) {
	printf("invoke HkRagdollConstraintData_GetPlaneMinAngularLimit\n");
	return __PVE_HkRagdollConstraintData_GetPlaneMinAngularLimit(instance);
}

void (*__PVE_HkRagdollConstraintData_SetPlaneMinAngularLimit)(void * instance, float value) __attribute__((ms_abi));
void HkRagdollConstraintData_SetPlaneMinAngularLimit(void * instance, float value) {
	printf("invoke HkRagdollConstraintData_SetPlaneMinAngularLimit\n");
	return __PVE_HkRagdollConstraintData_SetPlaneMinAngularLimit(instance, value);
}

float (*__PVE_HkRagdollConstraintData_GetPlaneMaxAngularLimit)(void * instance) __attribute__((ms_abi));
float HkRagdollConstraintData_GetPlaneMaxAngularLimit(void * instance) {
	printf("invoke HkRagdollConstraintData_GetPlaneMaxAngularLimit\n");
	return __PVE_HkRagdollConstraintData_GetPlaneMaxAngularLimit(instance);
}

void (*__PVE_HkRagdollConstraintData_SetPlaneMaxAngularLimit)(void * instance, float value) __attribute__((ms_abi));
void HkRagdollConstraintData_SetPlaneMaxAngularLimit(void * instance, float value) {
	printf("invoke HkRagdollConstraintData_SetPlaneMaxAngularLimit\n");
	return __PVE_HkRagdollConstraintData_SetPlaneMaxAngularLimit(instance, value);
}

float (*__PVE_HkRagdollConstraintData_GetTwistMinAngularLimit)(void * instance) __attribute__((ms_abi));
float HkRagdollConstraintData_GetTwistMinAngularLimit(void * instance) {
	printf("invoke HkRagdollConstraintData_GetTwistMinAngularLimit\n");
	return __PVE_HkRagdollConstraintData_GetTwistMinAngularLimit(instance);
}

void (*__PVE_HkRagdollConstraintData_SetTwistMinAngularLimit)(void * instance, float value) __attribute__((ms_abi));
void HkRagdollConstraintData_SetTwistMinAngularLimit(void * instance, float value) {
	printf("invoke HkRagdollConstraintData_SetTwistMinAngularLimit\n");
	return __PVE_HkRagdollConstraintData_SetTwistMinAngularLimit(instance, value);
}

float (*__PVE_HkRagdollConstraintData_GetTwistMaxAngularLimit)(void * instance) __attribute__((ms_abi));
float HkRagdollConstraintData_GetTwistMaxAngularLimit(void * instance) {
	printf("invoke HkRagdollConstraintData_GetTwistMaxAngularLimit\n");
	return __PVE_HkRagdollConstraintData_GetTwistMaxAngularLimit(instance);
}

void (*__PVE_HkRagdollConstraintData_SetTwistMaxAngularLimit)(void * instance, float value) __attribute__((ms_abi));
void HkRagdollConstraintData_SetTwistMaxAngularLimit(void * instance, float value) {
	printf("invoke HkRagdollConstraintData_SetTwistMaxAngularLimit\n");
	return __PVE_HkRagdollConstraintData_SetTwistMaxAngularLimit(instance, value);
}

float (*__PVE_HkRagdollConstraintData_GetMaxFrictionTorque)(void * instance) __attribute__((ms_abi));
float HkRagdollConstraintData_GetMaxFrictionTorque(void * instance) {
	printf("invoke HkRagdollConstraintData_GetMaxFrictionTorque\n");
	return __PVE_HkRagdollConstraintData_GetMaxFrictionTorque(instance);
}

void (*__PVE_HkRagdollConstraintData_SetMaxFrictionTorque)(void * instance, float value) __attribute__((ms_abi));
void HkRagdollConstraintData_SetMaxFrictionTorque(void * instance, float value) {
	printf("invoke HkRagdollConstraintData_SetMaxFrictionTorque\n");
	return __PVE_HkRagdollConstraintData_SetMaxFrictionTorque(instance, value);
}

void (*__PVE_HkRagdollConstraintData_SetInBodySpaceInternal)(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 planeAxisA, struct Vector3 planeAxisB, struct Vector3 twistAxisA, struct Vector3 twistAxisB) __attribute__((ms_abi));
void HkRagdollConstraintData_SetInBodySpaceInternal(void * instance, struct Vector3 pivotA, struct Vector3 pivotB, struct Vector3 planeAxisA, struct Vector3 planeAxisB, struct Vector3 twistAxisA, struct Vector3 twistAxisB) {
	printf("invoke HkRagdollConstraintData_SetInBodySpaceInternal\n");
	return __PVE_HkRagdollConstraintData_SetInBodySpaceInternal(instance, pivotA, pivotB, planeAxisA, planeAxisB, twistAxisA, twistAxisB);
}

void (*__PVE_HkRagdollConstraintData_SetAsymmetricConeAngle)(void * instance, float coneMin, float coneMax) __attribute__((ms_abi));
void HkRagdollConstraintData_SetAsymmetricConeAngle(void * instance, float coneMin, float coneMax) {
	printf("invoke HkRagdollConstraintData_SetAsymmetricConeAngle\n");
	return __PVE_HkRagdollConstraintData_SetAsymmetricConeAngle(instance, coneMin, coneMax);
}

void (*__PVE_HkRagdollConstraintData_SetConeLimitStabilization)(void * instance, int enable) __attribute__((ms_abi));
void HkRagdollConstraintData_SetConeLimitStabilization(void * instance, int enable) {
	printf("invoke HkRagdollConstraintData_SetConeLimitStabilization\n");
	return __PVE_HkRagdollConstraintData_SetConeLimitStabilization(instance, enable);
}

void * (*__PVE_HkBoxShape_Create)(struct Vector3 halfExtents) __attribute__((ms_abi));
void * HkBoxShape_Create(struct Vector3 halfExtents) {
	printf("invoke HkBoxShape_Create\n");
	return __PVE_HkBoxShape_Create(halfExtents);
}

void * (*__PVE_HkBoxShape_CreateWithConvexRadius)(struct Vector3 halfExtents, float convexRadius) __attribute__((ms_abi));
void * HkBoxShape_CreateWithConvexRadius(struct Vector3 halfExtents, float convexRadius) {
	printf("invoke HkBoxShape_CreateWithConvexRadius\n");
	return __PVE_HkBoxShape_CreateWithConvexRadius(halfExtents, convexRadius);
}

void * (*__PVE_HkBoxShape_GetShapeFromCompoundShape)(void * shape, int shapeIndex) __attribute__((ms_abi));
void * HkBoxShape_GetShapeFromCompoundShape(void * shape, int shapeIndex) {
	printf("invoke HkBoxShape_GetShapeFromCompoundShape\n");
	return __PVE_HkBoxShape_GetShapeFromCompoundShape(shape, shapeIndex);
}

struct Vector3 (*__PVE_HkBoxShape_GetHalfExtents)(void * instance) __attribute__((ms_abi));
struct Vector3 HkBoxShape_GetHalfExtents(void * instance) {
	printf("invoke HkBoxShape_GetHalfExtents\n");
	return __PVE_HkBoxShape_GetHalfExtents(instance);
}

void (*__PVE_HkBoxShape_SetHalfExtents)(void * instance, struct Vector3 value) __attribute__((ms_abi));
void HkBoxShape_SetHalfExtents(void * instance, struct Vector3 value) {
	printf("invoke HkBoxShape_SetHalfExtents\n");
	return __PVE_HkBoxShape_SetHalfExtents(instance, value);
}

void * (*__PVE_HkBvCompressedMeshShape_CreateWithSimpleMesh)(void * simpleMeshShape) __attribute__((ms_abi));
void * HkBvCompressedMeshShape_CreateWithSimpleMesh(void * simpleMeshShape) {
	printf("invoke HkBvCompressedMeshShape_CreateWithSimpleMesh\n");
	return __PVE_HkBvCompressedMeshShape_CreateWithSimpleMesh(simpleMeshShape);
}

void * (*__PVE_HkBvCompressedMeshShape_CreateWithParams)(void * geometry, int sCount, void * shapes, int tCount, void * transforms, int weldingType, int dataMode, int isWithConvexRadius, float convexRadius) __attribute__((ms_abi));
void * HkBvCompressedMeshShape_CreateWithParams(void * geometry, int sCount, void * shapes, int tCount, void * transforms, int weldingType, int dataMode, int isWithConvexRadius, float convexRadius) {
	printf("invoke HkBvCompressedMeshShape_CreateWithParams\n");
	return __PVE_HkBvCompressedMeshShape_CreateWithParams(geometry, sCount, shapes, tCount, transforms, weldingType, dataMode, isWithConvexRadius, convexRadius);
}

void * (*__PVE_HkBvCompressedMeshShape_CreateUnsafe)(void * vertices, int verticesCount, void * indices, int indicesCount, void * materials, int materialsCount, int weldingType, float convexRadius) __attribute__((ms_abi));
void * HkBvCompressedMeshShape_CreateUnsafe(void * vertices, int verticesCount, void * indices, int indicesCount, void * materials, int materialsCount, int weldingType, float convexRadius) {
	printf("invoke HkBvCompressedMeshShape_CreateUnsafe\n");
	return __PVE_HkBvCompressedMeshShape_CreateUnsafe(vertices, verticesCount, indices, indicesCount, materials, materialsCount, weldingType, convexRadius);
}

void (*__PVE_HkBvCompressedMeshShape_GetGeometry)(void * instance, void * geometry) __attribute__((ms_abi));
void HkBvCompressedMeshShape_GetGeometry(void * instance, void * geometry) {
	printf("invoke HkBvCompressedMeshShape_GetGeometry\n");
	return __PVE_HkBvCompressedMeshShape_GetGeometry(instance, geometry);
}

int (*__PVE_HkBvCompressedMeshShape_GetUserData)(void * instance, int shapeKey) __attribute__((ms_abi));
int HkBvCompressedMeshShape_GetUserData(void * instance, int shapeKey) {
	printf("invoke HkBvCompressedMeshShape_GetUserData\n");
	return __PVE_HkBvCompressedMeshShape_GetUserData(instance, shapeKey);
}

void * (*__PVE_HkBvShape_Create)(void * boundingVolumeShape, void * childShape) __attribute__((ms_abi));
void * HkBvShape_Create(void * boundingVolumeShape, void * childShape) {
	printf("invoke HkBvShape_Create\n");
	return __PVE_HkBvShape_Create(boundingVolumeShape, childShape);
}

void * (*__PVE_HkBvShape_GetChildShape)(void * instance) __attribute__((ms_abi));
void * HkBvShape_GetChildShape(void * instance) {
	printf("invoke HkBvShape_GetChildShape\n");
	return __PVE_HkBvShape_GetChildShape(instance);
}

void * (*__PVE_HkBvShape_GetBoundingVolumeShape)(void * instance) __attribute__((ms_abi));
void * HkBvShape_GetBoundingVolumeShape(void * instance) {
	printf("invoke HkBvShape_GetBoundingVolumeShape\n");
	return __PVE_HkBvShape_GetBoundingVolumeShape(instance);
}

void * (*__PVE_HkCapsuleShape_Create)(struct Vector3 vertexA, struct Vector3 vertexB, float radius) __attribute__((ms_abi));
void * HkCapsuleShape_Create(struct Vector3 vertexA, struct Vector3 vertexB, float radius) {
	printf("invoke HkCapsuleShape_Create\n");
	return __PVE_HkCapsuleShape_Create(vertexA, vertexB, radius);
}

float (*__PVE_HkCapsuleShape_GetRadius)(void * instance) __attribute__((ms_abi));
float HkCapsuleShape_GetRadius(void * instance) {
	printf("invoke HkCapsuleShape_GetRadius\n");
	return __PVE_HkCapsuleShape_GetRadius(instance);
}

struct Vector3 (*__PVE_HkCapsuleShape_GetVertexB)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCapsuleShape_GetVertexB(void * instance) {
	printf("invoke HkCapsuleShape_GetVertexB\n");
	return __PVE_HkCapsuleShape_GetVertexB(instance);
}

struct Vector3 (*__PVE_HkCapsuleShape_GetVertexA)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCapsuleShape_GetVertexA(void * instance) {
	printf("invoke HkCapsuleShape_GetVertexA\n");
	return __PVE_HkCapsuleShape_GetVertexA(instance);
}

struct Vector3 (*__PVE_HkCapsuleShape_GetCentre)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCapsuleShape_GetCentre(void * instance) {
	printf("invoke HkCapsuleShape_GetCentre\n");
	return __PVE_HkCapsuleShape_GetCentre(instance);
}

void * (*__PVE_HkConvexShape_GetConvexShapeFromCompoundShape)(void * shape, int shapeIndex) __attribute__((ms_abi));
void * HkConvexShape_GetConvexShapeFromCompoundShape(void * shape, int shapeIndex) {
	printf("invoke HkConvexShape_GetConvexShapeFromCompoundShape\n");
	return __PVE_HkConvexShape_GetConvexShapeFromCompoundShape(shape, shapeIndex);
}

float (*__PVE_HkConvexShape_GetConvexRadius)(void * instance) __attribute__((ms_abi));
float HkConvexShape_GetConvexRadius(void * instance) {
	printf("invoke HkConvexShape_GetConvexRadius\n");
	return __PVE_HkConvexShape_GetConvexRadius(instance);
}

void (*__PVE_HkConvexShape_SetConvexRadius)(void * instance, float value) __attribute__((ms_abi));
void HkConvexShape_SetConvexRadius(void * instance, float value) {
	printf("invoke HkConvexShape_SetConvexRadius\n");
	return __PVE_HkConvexShape_SetConvexRadius(instance, value);
}

float (*__PVE_HkConvexShape_GetDefaultConvexRadius)() __attribute__((ms_abi));
float HkConvexShape_GetDefaultConvexRadius() {
	printf("invoke HkConvexShape_GetDefaultConvexRadius\n");
	return __PVE_HkConvexShape_GetDefaultConvexRadius();
}

void * (*__PVE_HkConvexTransformShape_Create)(void * childShape, struct Matrix transform, int refPolicy) __attribute__((ms_abi));
void * HkConvexTransformShape_Create(void * childShape, struct Matrix transform, int refPolicy) {
	printf("invoke HkConvexTransformShape_Create\n");
	return __PVE_HkConvexTransformShape_Create(childShape, transform, refPolicy);
}

void * (*__PVE_HkConvexTransformShape_CreateTranslated)(void * childShape, struct Vector3 translation, struct Quaternion rotation, struct Vector3 scale, int refPolicy) __attribute__((ms_abi));
void * HkConvexTransformShape_CreateTranslated(void * childShape, struct Vector3 translation, struct Quaternion rotation, struct Vector3 scale, int refPolicy) {
	printf("invoke HkConvexTransformShape_CreateTranslated\n");
	return __PVE_HkConvexTransformShape_CreateTranslated(childShape, translation, rotation, scale, refPolicy);
}

void * (*__PVE_HkConvexTransformShape_GetChildShape)(void * instance) __attribute__((ms_abi));
void * HkConvexTransformShape_GetChildShape(void * instance) {
	printf("invoke HkConvexTransformShape_GetChildShape\n");
	return __PVE_HkConvexTransformShape_GetChildShape(instance);
}

struct Matrix (*__PVE_HkConvexTransformShape_GetTransform)(void * instance) __attribute__((ms_abi));
struct Matrix HkConvexTransformShape_GetTransform(void * instance) {
	printf("invoke HkConvexTransformShape_GetTransform\n");
	return __PVE_HkConvexTransformShape_GetTransform(instance);
}

void * (*__PVE_HkConvexTranslateShape_CreateWithChild)(void * childShape, struct Vector3 translation, int refPolicy) __attribute__((ms_abi));
void * HkConvexTranslateShape_CreateWithChild(void * childShape, struct Vector3 translation, int refPolicy) {
	printf("invoke HkConvexTranslateShape_CreateWithChild\n");
	return __PVE_HkConvexTranslateShape_CreateWithChild(childShape, translation, refPolicy);
}

void * (*__PVE_HkConvexTranslateShape_GetChildShape)(void * instance) __attribute__((ms_abi));
void * HkConvexTranslateShape_GetChildShape(void * instance) {
	printf("invoke HkConvexTranslateShape_GetChildShape\n");
	return __PVE_HkConvexTranslateShape_GetChildShape(instance);
}

struct Vector3 (*__PVE_HkConvexTranslateShape_GetTranslation)(void * instance) __attribute__((ms_abi));
struct Vector3 HkConvexTranslateShape_GetTranslation(void * instance) {
	printf("invoke HkConvexTranslateShape_GetTranslation\n");
	return __PVE_HkConvexTranslateShape_GetTranslation(instance);
}

void * (*__PVE_HkConvexVerticesShape_Create)(void * verts, int count) __attribute__((ms_abi));
void * HkConvexVerticesShape_Create(void * verts, int count) {
	printf("invoke HkConvexVerticesShape_Create\n");
	return __PVE_HkConvexVerticesShape_Create(verts, count);
}

void * (*__PVE_HkConvexVerticesShape_CreateWithRadius)(void * verts, int count, int shrink, float convexRadius) __attribute__((ms_abi));
void * HkConvexVerticesShape_CreateWithRadius(void * verts, int count, int shrink, float convexRadius) {
	printf("invoke HkConvexVerticesShape_CreateWithRadius\n");
	return __PVE_HkConvexVerticesShape_CreateWithRadius(verts, count, shrink, convexRadius);
}

struct Vector3 (*__PVE_HkConvexVerticesShape_GetCenter)(void * instance) __attribute__((ms_abi));
struct Vector3 HkConvexVerticesShape_GetCenter(void * instance) {
	printf("invoke HkConvexVerticesShape_GetCenter\n");
	return __PVE_HkConvexVerticesShape_GetCenter(instance);
}

int (*__PVE_HkConvexVerticesShape_GetVertexCount)(void * instance) __attribute__((ms_abi));
int HkConvexVerticesShape_GetVertexCount(void * instance) {
	printf("invoke HkConvexVerticesShape_GetVertexCount\n");
	return __PVE_HkConvexVerticesShape_GetVertexCount(instance);
}

int (*__PVE_HkConvexVerticesShape_GetFaceCount)(void * instance) __attribute__((ms_abi));
int HkConvexVerticesShape_GetFaceCount(void * instance) {
	printf("invoke HkConvexVerticesShape_GetFaceCount\n");
	return __PVE_HkConvexVerticesShape_GetFaceCount(instance);
}

void (*__PVE_HkConvexVerticesShape_GetFaces)(void * instance, void * faceIndexCount, void * faceIndices, void * faceCount, void * faceVertexCounts) __attribute__((ms_abi));
void HkConvexVerticesShape_GetFaces(void * instance, void * faceIndexCount, void * faceIndices, void * faceCount, void * faceVertexCounts) {
	printf("invoke HkConvexVerticesShape_GetFaces\n");
	return __PVE_HkConvexVerticesShape_GetFaces(instance, faceIndexCount, faceIndices, faceCount, faceVertexCounts);
}

void (*__PVE_HkConvexVerticesShape_GetVertices)(void * instance, void * vertexBuffer) __attribute__((ms_abi));
void HkConvexVerticesShape_GetVertices(void * instance, void * vertexBuffer) {
	printf("invoke HkConvexVerticesShape_GetVertices\n");
	return __PVE_HkConvexVerticesShape_GetVertices(instance, vertexBuffer);
}

void (*__PVE_HkConvexVerticesShape_GetGeometry)(void * instance, void * geometry, void * center) __attribute__((ms_abi));
void HkConvexVerticesShape_GetGeometry(void * instance, void * geometry, void * center) {
	printf("invoke HkConvexVerticesShape_GetGeometry\n");
	return __PVE_HkConvexVerticesShape_GetGeometry(instance, geometry, center);
}

void * (*__PVE_HkCylinderShape_Create)(struct Vector3 vertexA, struct Vector3 vertexB, float cylinderRadius) __attribute__((ms_abi));
void * HkCylinderShape_Create(struct Vector3 vertexA, struct Vector3 vertexB, float cylinderRadius) {
	printf("invoke HkCylinderShape_Create\n");
	return __PVE_HkCylinderShape_Create(vertexA, vertexB, cylinderRadius);
}

void * (*__PVE_HkCylinderShape_CreateWithConvexRadius)(struct Vector3 vertexA, struct Vector3 vertexB, float cylinderRadius, float convexRadius) __attribute__((ms_abi));
void * HkCylinderShape_CreateWithConvexRadius(struct Vector3 vertexA, struct Vector3 vertexB, float cylinderRadius, float convexRadius) {
	printf("invoke HkCylinderShape_CreateWithConvexRadius\n");
	return __PVE_HkCylinderShape_CreateWithConvexRadius(vertexA, vertexB, cylinderRadius, convexRadius);
}

struct Vector3 (*__PVE_HkCylinderShape_GetVertexB)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCylinderShape_GetVertexB(void * instance) {
	printf("invoke HkCylinderShape_GetVertexB\n");
	return __PVE_HkCylinderShape_GetVertexB(instance);
}

struct Vector3 (*__PVE_HkCylinderShape_GetVertexA)(void * instance) __attribute__((ms_abi));
struct Vector3 HkCylinderShape_GetVertexA(void * instance) {
	printf("invoke HkCylinderShape_GetVertexA\n");
	return __PVE_HkCylinderShape_GetVertexA(instance);
}

void (*__PVE_HkCylinderShape_SetVertexB)(void * instance, struct Vector3 vertex) __attribute__((ms_abi));
void HkCylinderShape_SetVertexB(void * instance, struct Vector3 vertex) {
	printf("invoke HkCylinderShape_SetVertexB\n");
	return __PVE_HkCylinderShape_SetVertexB(instance, vertex);
}

void (*__PVE_HkCylinderShape_SetVertexA)(void * instance, struct Vector3 vertex) __attribute__((ms_abi));
void HkCylinderShape_SetVertexA(void * instance, struct Vector3 vertex) {
	printf("invoke HkCylinderShape_SetVertexA\n");
	return __PVE_HkCylinderShape_SetVertexA(instance, vertex);
}

float (*__PVE_HkCylinderShape_GetRadius)(void * instance) __attribute__((ms_abi));
float HkCylinderShape_GetRadius(void * instance) {
	printf("invoke HkCylinderShape_GetRadius\n");
	return __PVE_HkCylinderShape_GetRadius(instance);
}

void (*__PVE_HkCylinderShape_SetRadius)(void * instance, float radius) __attribute__((ms_abi));
void HkCylinderShape_SetRadius(void * instance, float radius) {
	printf("invoke HkCylinderShape_SetRadius\n");
	return __PVE_HkCylinderShape_SetRadius(instance, radius);
}

void (*__PVE_HkCylinderShape_SetNumberOfVirtualSideSegments)(int number) __attribute__((ms_abi));
void HkCylinderShape_SetNumberOfVirtualSideSegments(int number) {
	printf("invoke HkCylinderShape_SetNumberOfVirtualSideSegments\n");
	return __PVE_HkCylinderShape_SetNumberOfVirtualSideSegments(number);
}

void * (*__PVE_HkGridShape_Create)(float cellSize, int policy) __attribute__((ms_abi));
void * HkGridShape_Create(float cellSize, int policy) {
	printf("invoke HkGridShape_Create\n");
	return __PVE_HkGridShape_Create(cellSize, policy);
}

float (*__PVE_HkGridShape_GetCellSize)(void * instance) __attribute__((ms_abi));
float HkGridShape_GetCellSize(void * instance) {
	printf("invoke HkGridShape_GetCellSize\n");
	return __PVE_HkGridShape_GetCellSize(instance);
}

int (*__PVE_HkGridShape_GetShapeCount)(void * instance) __attribute__((ms_abi));
int HkGridShape_GetShapeCount(void * instance) {
	printf("invoke HkGridShape_GetShapeCount\n");
	return __PVE_HkGridShape_GetShapeCount(instance);
}

void (*__PVE_HkGridShape_SetDebugRigidBody)(void * instance, void * rigidBody) __attribute__((ms_abi));
void HkGridShape_SetDebugRigidBody(void * instance, void * rigidBody) {
	printf("invoke HkGridShape_SetDebugRigidBody\n");
	return __PVE_HkGridShape_SetDebugRigidBody(instance, rigidBody);
}

void * (*__PVE_HkGridShape_GetDebugRigidBody)(void * instance) __attribute__((ms_abi));
void * HkGridShape_GetDebugRigidBody(void * instance) {
	printf("invoke HkGridShape_GetDebugRigidBody\n");
	return __PVE_HkGridShape_GetDebugRigidBody(instance);
}

void (*__PVE_HkGridShape_SetDebugDraw)(void * instance, int debugDraw) __attribute__((ms_abi));
void HkGridShape_SetDebugDraw(void * instance, int debugDraw) {
	printf("invoke HkGridShape_SetDebugDraw\n");
	return __PVE_HkGridShape_SetDebugDraw(instance, debugDraw);
}

int (*__PVE_HkGridShape_GetDebugDraw)(void * instance) __attribute__((ms_abi));
int HkGridShape_GetDebugDraw(void * instance) {
	printf("invoke HkGridShape_GetDebugDraw\n");
	return __PVE_HkGridShape_GetDebugDraw(instance);
}

void (*__PVE_HkGridShape_AddShapes)(void * instance, void * shapes, int count, struct Vector3S min, struct Vector3S max) __attribute__((ms_abi));
void HkGridShape_AddShapes(void * instance, void * shapes, int count, struct Vector3S min, struct Vector3S max) {
	printf("invoke HkGridShape_AddShapes\n");
	return __PVE_HkGridShape_AddShapes(instance, shapes, count, min, max);
}

int (*__PVE_HkGridShape_Contains)(void * instance, short x, short y, short z) __attribute__((ms_abi));
int HkGridShape_Contains(void * instance, short x, short y, short z) {
	printf("invoke HkGridShape_Contains\n");
	return __PVE_HkGridShape_Contains(instance, x, y, z);
}

void (*__PVE_HkGridShape_GetShape)(void * instance, struct Vector3I pos, void * shapeBuffer) __attribute__((ms_abi));
void HkGridShape_GetShape(void * instance, struct Vector3I pos, void * shapeBuffer) {
	printf("invoke HkGridShape_GetShape\n");
	return __PVE_HkGridShape_GetShape(instance, pos, shapeBuffer);
}

void (*__PVE_HkGridShape_GetShapeInfo)(void * instance, int index, void * min, void * max, void * shapeBuffer) __attribute__((ms_abi));
void HkGridShape_GetShapeInfo(void * instance, int index, void * min, void * max, void * shapeBuffer) {
	printf("invoke HkGridShape_GetShapeInfo\n");
	return __PVE_HkGridShape_GetShapeInfo(instance, index, min, max, shapeBuffer);
}

int (*__PVE_HkGridShape_GetShapeInfoCount)(void * instance) __attribute__((ms_abi));
int HkGridShape_GetShapeInfoCount(void * instance) {
	printf("invoke HkGridShape_GetShapeInfoCount\n");
	return __PVE_HkGridShape_GetShapeInfoCount(instance);
}

void (*__PVE_HkGridShape_GetShapeMin)(void * instance, int shapeKey, void * min) __attribute__((ms_abi));
void HkGridShape_GetShapeMin(void * instance, int shapeKey, void * min) {
	printf("invoke HkGridShape_GetShapeMin\n");
	return __PVE_HkGridShape_GetShapeMin(instance, shapeKey, min);
}

void (*__PVE_HkGridShape_GetShapesInInterval)(void * instance, struct Vector3 min, struct Vector3 max, void * shapeBuffer) __attribute__((ms_abi));
void HkGridShape_GetShapesInInterval(void * instance, struct Vector3 min, struct Vector3 max, void * shapeBuffer) {
	printf("invoke HkGridShape_GetShapesInInterval\n");
	return __PVE_HkGridShape_GetShapesInInterval(instance, min, max, shapeBuffer);
}

void (*__PVE_HkGridShape_GetChildBounds)(void * instance, int shapeKey, void * min, void * max) __attribute__((ms_abi));
void HkGridShape_GetChildBounds(void * instance, int shapeKey, void * min, void * max) {
	printf("invoke HkGridShape_GetChildBounds\n");
	return __PVE_HkGridShape_GetChildBounds(instance, shapeKey, min, max);
}

void (*__PVE_HkGridShape_RemoveShapes)(void * instance, void * positions, int count, void * results) __attribute__((ms_abi));
void HkGridShape_RemoveShapes(void * instance, void * positions, int count, void * results) {
	printf("invoke HkGridShape_RemoveShapes\n");
	return __PVE_HkGridShape_RemoveShapes(instance, positions, count, results);
}

void (*__PVE_HkGridShape_GetCellRanges)(void * instance, void * positions, int count, void * results) __attribute__((ms_abi));
void HkGridShape_GetCellRanges(void * instance, void * positions, int count, void * results) {
	printf("invoke HkGridShape_GetCellRanges\n");
	return __PVE_HkGridShape_GetCellRanges(instance, positions, count, results);
}

void * (*__PVE_HkListShape_Create)(void * shapes, int count, int refPolicy) __attribute__((ms_abi));
void * HkListShape_Create(void * shapes, int count, int refPolicy) {
	printf("invoke HkListShape_Create\n");
	return __PVE_HkListShape_Create(shapes, count, refPolicy);
}

short (*__PVE_HkListShape_GetDisabledChildrenCount)(void * instance) __attribute__((ms_abi));
short HkListShape_GetDisabledChildrenCount(void * instance) {
	printf("invoke HkListShape_GetDisabledChildrenCount\n");
	return __PVE_HkListShape_GetDisabledChildrenCount(instance);
}

int (*__PVE_HkListShape_GetTotalChildrenCount)(void * instance) __attribute__((ms_abi));
int HkListShape_GetTotalChildrenCount(void * instance) {
	printf("invoke HkListShape_GetTotalChildrenCount\n");
	return __PVE_HkListShape_GetTotalChildrenCount(instance);
}

void (*__PVE_HkListShape_EnableShape)(void * instance, int shapeKey, int isEnable) __attribute__((ms_abi));
void HkListShape_EnableShape(void * instance, int shapeKey, int isEnable) {
	printf("invoke HkListShape_EnableShape\n");
	return __PVE_HkListShape_EnableShape(instance, shapeKey, isEnable);
}

void * (*__PVE_HkListShape_GetChildByIndex)(void * instance, int index) __attribute__((ms_abi));
void * HkListShape_GetChildByIndex(void * instance, int index) {
	printf("invoke HkListShape_GetChildByIndex\n");
	return __PVE_HkListShape_GetChildByIndex(instance, index);
}

int (*__PVE_HkListShape_IsChildEnabled)(void * instance, int shapeKey) __attribute__((ms_abi));
int HkListShape_IsChildEnabled(void * instance, int shapeKey) {
	printf("invoke HkListShape_IsChildEnabled\n");
	return __PVE_HkListShape_IsChildEnabled(instance, shapeKey);
}

void * (*__PVE_HkMoppBvTreeShape_Create)(void * shapeCollection) __attribute__((ms_abi));
void * HkMoppBvTreeShape_Create(void * shapeCollection) {
	printf("invoke HkMoppBvTreeShape_Create\n");
	return __PVE_HkMoppBvTreeShape_Create(shapeCollection);
}

void * (*__PVE_HkMoppBvTreeShape_GetShapeCollection)(void * instance) __attribute__((ms_abi));
void * HkMoppBvTreeShape_GetShapeCollection(void * instance) {
	printf("invoke HkMoppBvTreeShape_GetShapeCollection\n");
	return __PVE_HkMoppBvTreeShape_GetShapeCollection(instance);
}

void (*__PVE_HkMoppBvTreeShape_DisableKeys)(void * instance, void * keys, int count) __attribute__((ms_abi));
void HkMoppBvTreeShape_DisableKeys(void * instance, void * keys, int count) {
	printf("invoke HkMoppBvTreeShape_DisableKeys\n");
	return __PVE_HkMoppBvTreeShape_DisableKeys(instance, keys, count);
}

void (*__PVE_HkMoppBvTreeShape_QueryAABB)(void * instance, struct Vector3 min, struct Vector3 max, void * shapeKeys) __attribute__((ms_abi));
void HkMoppBvTreeShape_QueryAABB(void * instance, struct Vector3 min, struct Vector3 max, void * shapeKeys) {
	printf("invoke HkMoppBvTreeShape_QueryAABB\n");
	return __PVE_HkMoppBvTreeShape_QueryAABB(instance, min, max, shapeKeys);
}

void (*__PVE_HkMoppBvTreeShape_QueryPoint)(void * instance, struct Vector3 point, void * shapeKeys) __attribute__((ms_abi));
void HkMoppBvTreeShape_QueryPoint(void * instance, struct Vector3 point, void * shapeKeys) {
	printf("invoke HkMoppBvTreeShape_QueryPoint\n");
	return __PVE_HkMoppBvTreeShape_QueryPoint(instance, point, shapeKeys);
}

void * (*__PVE_HkPhantomCallbackShape_Create)(void * enterCallback, void * leaveCallback, void * deleteCallback) __attribute__((ms_abi));
void * HkPhantomCallbackShape_Create(void * enterCallback, void * leaveCallback, void * deleteCallback) {
	printf("invoke HkPhantomCallbackShape_Create\n");
	return __PVE_HkPhantomCallbackShape_Create(_PVE_Trampoline_Havok_HkPhantomCallbackShape_HkPhantomHandlerCpp(enterCallback), _PVE_Trampoline_Havok_HkPhantomCallbackShape_HkPhantomHandlerCpp(leaveCallback), _PVE_Trampoline_Havok_HkDeleteHandler(deleteCallback));
}

int (*__PVE_HkShape_GetReferenceCount)(void * instance) __attribute__((ms_abi));
int HkShape_GetReferenceCount(void * instance) {
	printf("invoke HkShape_GetReferenceCount\n");
	return __PVE_HkShape_GetReferenceCount(instance);
}

int (*__PVE_HkShape_GetShapeType)(void * instance) __attribute__((ms_abi));
int HkShape_GetShapeType(void * instance) {
	printf("invoke HkShape_GetShapeType\n");
	return __PVE_HkShape_GetShapeType(instance);
}

int (*__PVE_HkShape_IsConvex)(void * instance) __attribute__((ms_abi));
int HkShape_IsConvex(void * instance) {
	printf("invoke HkShape_IsConvex\n");
	return __PVE_HkShape_IsConvex(instance);
}

float (*__PVE_HkShape_GetConvexRadius)(void * instance) __attribute__((ms_abi));
float HkShape_GetConvexRadius(void * instance) {
	printf("invoke HkShape_GetConvexRadius\n");
	return __PVE_HkShape_GetConvexRadius(instance);
}

void (*__PVE_HkShape_SetConvexRadius)(void * instance, float value) __attribute__((ms_abi));
void HkShape_SetConvexRadius(void * instance, float value) {
	printf("invoke HkShape_SetConvexRadius\n");
	return __PVE_HkShape_SetConvexRadius(instance, value);
}

long int (*__PVE_HkShape_GetUserData)(void * instance) __attribute__((ms_abi));
long int HkShape_GetUserData(void * instance) {
	printf("invoke HkShape_GetUserData\n");
	return __PVE_HkShape_GetUserData(instance);
}

void (*__PVE_HkShape_SetUserData)(void * instance, long int value) __attribute__((ms_abi));
void HkShape_SetUserData(void * instance, long int value) {
	printf("invoke HkShape_SetUserData\n");
	return __PVE_HkShape_SetUserData(instance, value);
}

void (*__PVE_HkShape_SetRigidBody)(void * instance, void * rigidBody) __attribute__((ms_abi));
void HkShape_SetRigidBody(void * instance, void * rigidBody) {
	printf("invoke HkShape_SetRigidBody\n");
	return __PVE_HkShape_SetRigidBody(instance, rigidBody);
}

int (*__PVE_HkShape_IsContainer)(void * instance) __attribute__((ms_abi));
int HkShape_IsContainer(void * instance) {
	printf("invoke HkShape_IsContainer\n");
	return __PVE_HkShape_IsContainer(instance);
}

void (*__PVE_HkShape_AddReference)(void * instance) __attribute__((ms_abi));
void HkShape_AddReference(void * instance) {
	printf("invoke HkShape_AddReference\n");
	return __PVE_HkShape_AddReference(instance);
}

void (*__PVE_HkShape_RemoveReference)(void * instance) __attribute__((ms_abi));
void HkShape_RemoveReference(void * instance) {
	printf("invoke HkShape_RemoveReference\n");
	return __PVE_HkShape_RemoveReference(instance);
}

void (*__PVE_HkShape_DisableRefCount)(void * instance) __attribute__((ms_abi));
void HkShape_DisableRefCount(void * instance) {
	printf("invoke HkShape_DisableRefCount\n");
	return __PVE_HkShape_DisableRefCount(instance);
}

void (*__PVE_HkShape_GetLocalAABB)(void * instance, float tolerance, void * outMin, void * outMax) __attribute__((ms_abi));
void HkShape_GetLocalAABB(void * instance, float tolerance, void * outMin, void * outMax) {
	printf("invoke HkShape_GetLocalAABB\n");
	return __PVE_HkShape_GetLocalAABB(instance, tolerance, outMin, outMax);
}

int (*__PVE_HkShape_CastRayCollectSingleHit)(void * instance, struct Vector3 from, struct Vector3 to) __attribute__((ms_abi));
int HkShape_CastRayCollectSingleHit(void * instance, struct Vector3 from, struct Vector3 to) {
	printf("invoke HkShape_CastRayCollectSingleHit\n");
	return __PVE_HkShape_CastRayCollectSingleHit(instance, from, to);
}

void * (*__PVE_HkShape_LoadShapeFromFile)(void * filename) __attribute__((ms_abi));
void * HkShape_LoadShapeFromFile(void * filename) {
	printf("invoke HkShape_LoadShapeFromFile\n");
	return __PVE_HkShape_LoadShapeFromFile(filename);
}

void * (*__PVE_HkShape_GetContainer)(void * instance) __attribute__((ms_abi));
void * HkShape_GetContainer(void * instance) {
	printf("invoke HkShape_GetContainer\n");
	return __PVE_HkShape_GetContainer(instance);
}

int (*__PVE_HkShapeBatch_GetCount)(int batchId) __attribute__((ms_abi));
int HkShapeBatch_GetCount(int batchId) {
	printf("invoke HkShapeBatch_GetCount\n");
	return __PVE_HkShapeBatch_GetCount(batchId);
}

void (*__PVE_HkShapeBatch_GetInfo)(int batchId, int shapeIndex, void * outPos) __attribute__((ms_abi));
void HkShapeBatch_GetInfo(int batchId, int shapeIndex, void * outPos) {
	printf("invoke HkShapeBatch_GetInfo\n");
	return __PVE_HkShapeBatch_GetInfo(batchId, shapeIndex, outPos);
}

void (*__PVE_HkShapeBatch_SetResult)(int batchId, int shapeIndex, void * shape) __attribute__((ms_abi));
void HkShapeBatch_SetResult(int batchId, int shapeIndex, void * shape) {
	printf("invoke HkShapeBatch_SetResult\n");
	return __PVE_HkShapeBatch_SetResult(batchId, shapeIndex, shape);
}

void * (*__PVE_HkShapeBuffer_Create)() __attribute__((ms_abi));
void * HkShapeBuffer_Create() {
	printf("invoke HkShapeBuffer_Create\n");
	return __PVE_HkShapeBuffer_Create();
}

void * (*__PVE_HkShapeBuffer_Destroy)(void * instance) __attribute__((ms_abi));
void * HkShapeBuffer_Destroy(void * instance) {
	printf("invoke HkShapeBuffer_Destroy\n");
	return __PVE_HkShapeBuffer_Destroy(instance);
}

int (*__PVE_HkShapeCollection_GetShapeCount)(void * instance) __attribute__((ms_abi));
int HkShapeCollection_GetShapeCount(void * instance) {
	printf("invoke HkShapeCollection_GetShapeCount\n");
	return __PVE_HkShapeCollection_GetShapeCount(instance);
}

void * (*__PVE_HkShapeCollection_GetShape)(void * instance, int shapeKey) __attribute__((ms_abi));
void * HkShapeCollection_GetShape(void * instance, int shapeKey) {
	printf("invoke HkShapeCollection_GetShape\n");
	return __PVE_HkShapeCollection_GetShape(instance, shapeKey);
}

void * (*__PVE_HkShapeCollection_GetShapeWithBuffer)(void * instance, int shapeKey, void * buffer) __attribute__((ms_abi));
void * HkShapeCollection_GetShapeWithBuffer(void * instance, int shapeKey, void * buffer) {
	printf("invoke HkShapeCollection_GetShapeWithBuffer\n");
	return __PVE_HkShapeCollection_GetShapeWithBuffer(instance, shapeKey, buffer);
}

int (*__PVE_HkShapeContainer_GetFirstKey)(void * instance) __attribute__((ms_abi));
int HkShapeContainer_GetFirstKey(void * instance) {
	printf("invoke HkShapeContainer_GetFirstKey\n");
	return __PVE_HkShapeContainer_GetFirstKey(instance);
}

int (*__PVE_HkShapeContainer_GetNextKey)(void * instance, int key) __attribute__((ms_abi));
int HkShapeContainer_GetNextKey(void * instance, int key) {
	printf("invoke HkShapeContainer_GetNextKey\n");
	return __PVE_HkShapeContainer_GetNextKey(instance, key);
}

void * (*__PVE_HkShapeContainer_CurrentValue)(void * instance, int key, void * buffer) __attribute__((ms_abi));
void * HkShapeContainer_CurrentValue(void * instance, int key, void * buffer) {
	printf("invoke HkShapeContainer_CurrentValue\n");
	return __PVE_HkShapeContainer_CurrentValue(instance, key, buffer);
}

void * (*__PVE_HkShapeContainer_GetShape)(void * instance, int key) __attribute__((ms_abi));
void * HkShapeContainer_GetShape(void * instance, int key) {
	printf("invoke HkShapeContainer_GetShape\n");
	return __PVE_HkShapeContainer_GetShape(instance, key);
}

int (*__PVE_HkShapeContainer_IsShapeKeyValid)(void * instance, int key) __attribute__((ms_abi));
int HkShapeContainer_IsShapeKeyValid(void * instance, int key) {
	printf("invoke HkShapeContainer_IsShapeKeyValid\n");
	return __PVE_HkShapeContainer_IsShapeKeyValid(instance, key);
}

int (*__PVE_HkShapeLoader_LoadShapesListFromBuffer)(int cBuffer, void * buffer, void * shapeBuffer, void * containsScene, void * containsDestruction) __attribute__((ms_abi));
int HkShapeLoader_LoadShapesListFromBuffer(int cBuffer, void * buffer, void * shapeBuffer, void * containsScene, void * containsDestruction) {
	printf("invoke HkShapeLoader_LoadShapesListFromBuffer\n");
	return __PVE_HkShapeLoader_LoadShapesListFromBuffer(cBuffer, buffer, shapeBuffer, containsScene, containsDestruction);
}

int (*__PVE_HkShapeLoader_LoadShapesListFromFile)(void * fileName, void * shapeBuffer) __attribute__((ms_abi));
int HkShapeLoader_LoadShapesListFromFile(void * fileName, void * shapeBuffer) {
	printf("invoke HkShapeLoader_LoadShapesListFromFile\n");
	return __PVE_HkShapeLoader_LoadShapesListFromFile(fileName, shapeBuffer);
}

int (*__PVE_HkShapeLoader_SaveShapesListToFile)(void * fileName, void * listShapes, int xmlFormat) __attribute__((ms_abi));
int HkShapeLoader_SaveShapesListToFile(void * fileName, void * listShapes, int xmlFormat) {
	printf("invoke HkShapeLoader_SaveShapesListToFile\n");
	return __PVE_HkShapeLoader_SaveShapesListToFile(fileName, listShapes, xmlFormat);
}

int (*__PVE_HkShapeLoader_CleanupShapesBuffer)(int cBuffer, void * buffer, void * returnByteArray) __attribute__((ms_abi));
int HkShapeLoader_CleanupShapesBuffer(int cBuffer, void * buffer, void * returnByteArray) {
	printf("invoke HkShapeLoader_CleanupShapesBuffer\n");
	return __PVE_HkShapeLoader_CleanupShapesBuffer(cBuffer, buffer, _PVE_Trampoline_Havok_HkShapeLoader_ReturnByteArray(returnByteArray));
}

void * (*__PVE_HkSimpleMeshShape_Create)(int vCount, void * vertices, int iCount, void * indices, int mCount, void * materials) __attribute__((ms_abi));
void * HkSimpleMeshShape_Create(int vCount, void * vertices, int iCount, void * indices, int mCount, void * materials) {
	printf("invoke HkSimpleMeshShape_Create\n");
	return __PVE_HkSimpleMeshShape_Create(vCount, vertices, iCount, indices, mCount, materials);
}

void * (*__PVE_HkSmartListShape_Create)(int dummy) __attribute__((ms_abi));
void * HkSmartListShape_Create(int dummy) {
	printf("invoke HkSmartListShape_Create\n");
	return __PVE_HkSmartListShape_Create(dummy);
}

int (*__PVE_HkSmartListShape_GetShapeCount)(void * instance) __attribute__((ms_abi));
int HkSmartListShape_GetShapeCount(void * instance) {
	printf("invoke HkSmartListShape_GetShapeCount\n");
	return __PVE_HkSmartListShape_GetShapeCount(instance);
}

void (*__PVE_HkSmartListShape_AddShape)(void * instance, void * shape) __attribute__((ms_abi));
void HkSmartListShape_AddShape(void * instance, void * shape) {
	printf("invoke HkSmartListShape_AddShape\n");
	return __PVE_HkSmartListShape_AddShape(instance, shape);
}

void (*__PVE_HkSmartListShape_RemoveShape)(void * instance, void * shape) __attribute__((ms_abi));
void HkSmartListShape_RemoveShape(void * instance, void * shape) {
	printf("invoke HkSmartListShape_RemoveShape\n");
	return __PVE_HkSmartListShape_RemoveShape(instance, shape);
}

void (*__PVE_HkSmartListShape_Validate)(void * instance) __attribute__((ms_abi));
void HkSmartListShape_Validate(void * instance) {
	printf("invoke HkSmartListShape_Validate\n");
	return __PVE_HkSmartListShape_Validate(instance);
}

void * (*__PVE_HkSphereShape_Create)(float radius) __attribute__((ms_abi));
void * HkSphereShape_Create(float radius) {
	printf("invoke HkSphereShape_Create\n");
	return __PVE_HkSphereShape_Create(radius);
}

float (*__PVE_HkSphereShape_GetRadius)(void * instance) __attribute__((ms_abi));
float HkSphereShape_GetRadius(void * instance) {
	printf("invoke HkSphereShape_GetRadius\n");
	return __PVE_HkSphereShape_GetRadius(instance);
}

void (*__PVE_HkSphereShape_SetRadius)(void * instance, float radius) __attribute__((ms_abi));
void HkSphereShape_SetRadius(void * instance, float radius) {
	printf("invoke HkSphereShape_SetRadius\n");
	return __PVE_HkSphereShape_SetRadius(instance, radius);
}

void * (*__PVE_HkStaticCompoundShape_Create)(int refPolicy) __attribute__((ms_abi));
void * HkStaticCompoundShape_Create(int refPolicy) {
	printf("invoke HkStaticCompoundShape_Create\n");
	return __PVE_HkStaticCompoundShape_Create(refPolicy);
}

int (*__PVE_HkStaticCompoundShape_GetInstanceCount)(void * instance) __attribute__((ms_abi));
int HkStaticCompoundShape_GetInstanceCount(void * instance) {
	printf("invoke HkStaticCompoundShape_GetInstanceCount\n");
	return __PVE_HkStaticCompoundShape_GetInstanceCount(instance);
}

int (*__PVE_HkStaticCompoundShape_AddInstance)(void * instance, void * shape, struct Matrix transform) __attribute__((ms_abi));
int HkStaticCompoundShape_AddInstance(void * instance, void * shape, struct Matrix transform) {
	printf("invoke HkStaticCompoundShape_AddInstance\n");
	return __PVE_HkStaticCompoundShape_AddInstance(instance, shape, transform);
}

void (*__PVE_HkStaticCompoundShape_Bake)(void * instance) __attribute__((ms_abi));
void HkStaticCompoundShape_Bake(void * instance) {
	printf("invoke HkStaticCompoundShape_Bake\n");
	return __PVE_HkStaticCompoundShape_Bake(instance);
}

int (*__PVE_HkStaticCompoundShape_ComposeShapeKey)(void * instance, int instanceId, int shapeKey) __attribute__((ms_abi));
int HkStaticCompoundShape_ComposeShapeKey(void * instance, int instanceId, int shapeKey) {
	printf("invoke HkStaticCompoundShape_ComposeShapeKey\n");
	return __PVE_HkStaticCompoundShape_ComposeShapeKey(instance, instanceId, shapeKey);
}

struct DecomposeShapeKeyResult (*__PVE_HkStaticCompoundShape_DecomposeShapeKey)(void * instance, int shapeKey) __attribute__((ms_abi));
struct DecomposeShapeKeyResult HkStaticCompoundShape_DecomposeShapeKey(void * instance, int shapeKey) {
	printf("invoke HkStaticCompoundShape_DecomposeShapeKey\n");
	return __PVE_HkStaticCompoundShape_DecomposeShapeKey(instance, shapeKey);
}

void (*__PVE_HkStaticCompoundShape_EnableAllShapeKeys)(void * instance) __attribute__((ms_abi));
void HkStaticCompoundShape_EnableAllShapeKeys(void * instance) {
	printf("invoke HkStaticCompoundShape_EnableAllShapeKeys\n");
	return __PVE_HkStaticCompoundShape_EnableAllShapeKeys(instance);
}

void (*__PVE_HkStaticCompoundShape_EnableInstance)(void * instance, int instanceId, int enable) __attribute__((ms_abi));
void HkStaticCompoundShape_EnableInstance(void * instance, int instanceId, int enable) {
	printf("invoke HkStaticCompoundShape_EnableInstance\n");
	return __PVE_HkStaticCompoundShape_EnableInstance(instance, instanceId, enable);
}

void (*__PVE_HkStaticCompoundShape_EnableShapeKey)(void * instance, int key, int enable) __attribute__((ms_abi));
void HkStaticCompoundShape_EnableShapeKey(void * instance, int key, int enable) {
	printf("invoke HkStaticCompoundShape_EnableShapeKey\n");
	return __PVE_HkStaticCompoundShape_EnableShapeKey(instance, key, enable);
}

int (*__PVE_HkStaticCompoundShape_GetFirstKey)(void * instance) __attribute__((ms_abi));
int HkStaticCompoundShape_GetFirstKey(void * instance) {
	printf("invoke HkStaticCompoundShape_GetFirstKey\n");
	return __PVE_HkStaticCompoundShape_GetFirstKey(instance);
}

void * (*__PVE_HkStaticCompoundShape_GetInstance)(void * instance, int instanceIndex) __attribute__((ms_abi));
void * HkStaticCompoundShape_GetInstance(void * instance, int instanceIndex) {
	printf("invoke HkStaticCompoundShape_GetInstance\n");
	return __PVE_HkStaticCompoundShape_GetInstance(instance, instanceIndex);
}

struct Matrix (*__PVE_HkStaticCompoundShape_GetInstanceTransform)(void * instance, int instanceIndex) __attribute__((ms_abi));
struct Matrix HkStaticCompoundShape_GetInstanceTransform(void * instance, int instanceIndex) {
	printf("invoke HkStaticCompoundShape_GetInstanceTransform\n");
	return __PVE_HkStaticCompoundShape_GetInstanceTransform(instance, instanceIndex);
}

int (*__PVE_HkStaticCompoundShape_IsInstanceEnabled)(void * instance, int instanceId) __attribute__((ms_abi));
int HkStaticCompoundShape_IsInstanceEnabled(void * instance, int instanceId) {
	printf("invoke HkStaticCompoundShape_IsInstanceEnabled\n");
	return __PVE_HkStaticCompoundShape_IsInstanceEnabled(instance, instanceId);
}

int (*__PVE_HkStaticCompoundShape_IsShapeKeyEnabled)(void * instance, int key) __attribute__((ms_abi));
int HkStaticCompoundShape_IsShapeKeyEnabled(void * instance, int key) {
	printf("invoke HkStaticCompoundShape_IsShapeKeyEnabled\n");
	return __PVE_HkStaticCompoundShape_IsShapeKeyEnabled(instance, key);
}

void * (*__PVE_HkTransformShape_Create)(void * childShape, struct Matrix transform) __attribute__((ms_abi));
void * HkTransformShape_Create(void * childShape, struct Matrix transform) {
	printf("invoke HkTransformShape_Create\n");
	return __PVE_HkTransformShape_Create(childShape, transform);
}

void * (*__PVE_HkTransformShape_CreateWithTranslation)(void * childShape, struct Vector3 translation, struct Quaternion rotation) __attribute__((ms_abi));
void * HkTransformShape_CreateWithTranslation(void * childShape, struct Vector3 translation, struct Quaternion rotation) {
	printf("invoke HkTransformShape_CreateWithTranslation\n");
	return __PVE_HkTransformShape_CreateWithTranslation(childShape, translation, rotation);
}

struct Matrix (*__PVE_HkTransformShape_GetTransform)(void * instance) __attribute__((ms_abi));
struct Matrix HkTransformShape_GetTransform(void * instance) {
	printf("invoke HkTransformShape_GetTransform\n");
	return __PVE_HkTransformShape_GetTransform(instance);
}

void * (*__PVE_HkTransformShape_GetChildShape)(void * instance) __attribute__((ms_abi));
void * HkTransformShape_GetChildShape(void * instance) {
	printf("invoke HkTransformShape_GetChildShape\n");
	return __PVE_HkTransformShape_GetChildShape(instance);
}

struct Vector3 (*__PVE_HkTriangleShape_GetExtrusion)(void * instance) __attribute__((ms_abi));
struct Vector3 HkTriangleShape_GetExtrusion(void * instance) {
	printf("invoke HkTriangleShape_GetExtrusion\n");
	return __PVE_HkTriangleShape_GetExtrusion(instance);
}

struct Vector3 (*__PVE_HkTriangleShape_GetPt2)(void * instance) __attribute__((ms_abi));
struct Vector3 HkTriangleShape_GetPt2(void * instance) {
	printf("invoke HkTriangleShape_GetPt2\n");
	return __PVE_HkTriangleShape_GetPt2(instance);
}

struct Vector3 (*__PVE_HkTriangleShape_GetPt1)(void * instance) __attribute__((ms_abi));
struct Vector3 HkTriangleShape_GetPt1(void * instance) {
	printf("invoke HkTriangleShape_GetPt1\n");
	return __PVE_HkTriangleShape_GetPt1(instance);
}

struct Vector3 (*__PVE_HkTriangleShape_GetPt0)(void * instance) __attribute__((ms_abi));
struct Vector3 HkTriangleShape_GetPt0(void * instance) {
	printf("invoke HkTriangleShape_GetPt0\n");
	return __PVE_HkTriangleShape_GetPt0(instance);
}

void * (*__PVE_HkUniformGridShape_Create)(struct HkUniformGridShapeArgsPOD argsPod) __attribute__((ms_abi));
void * HkUniformGridShape_Create(struct HkUniformGridShapeArgsPOD argsPod) {
	printf("invoke HkUniformGridShape_Create\n");
	return __PVE_HkUniformGridShape_Create(argsPod);
}

int (*__PVE_HkUniformGridShape_GetShapeCount)(void * instance) __attribute__((ms_abi));
int HkUniformGridShape_GetShapeCount(void * instance) {
	printf("invoke HkUniformGridShape_GetShapeCount\n");
	return __PVE_HkUniformGridShape_GetShapeCount(instance);
}

void (*__PVE_HkUniformGridShape_DiscardLargeData)(void * instance) __attribute__((ms_abi));
void HkUniformGridShape_DiscardLargeData(void * instance) {
	printf("invoke HkUniformGridShape_DiscardLargeData\n");
	return __PVE_HkUniformGridShape_DiscardLargeData(instance);
}

int (*__PVE_HkUniformGridShape_GetHitsAndClear)(void * instance) __attribute__((ms_abi));
int HkUniformGridShape_GetHitsAndClear(void * instance) {
	printf("invoke HkUniformGridShape_GetHitsAndClear\n");
	return __PVE_HkUniformGridShape_GetHitsAndClear(instance);
}

int (*__PVE_HkUniformGridShape_GetHitCellsInRange)(void * instance, struct Vector3 min, struct Vector3 max, int bufferSize, void * buffer) __attribute__((ms_abi));
int HkUniformGridShape_GetHitCellsInRange(void * instance, struct Vector3 min, struct Vector3 max, int bufferSize, void * buffer) {
	printf("invoke HkUniformGridShape_GetHitCellsInRange\n");
	return __PVE_HkUniformGridShape_GetHitCellsInRange(instance, min, max, bufferSize, buffer);
}

int (*__PVE_HkUniformGridShape_GetMissingCellsInRange)(void * instance, struct Vector3 min, struct Vector3 max, int bufferSize, void * buffer) __attribute__((ms_abi));
int HkUniformGridShape_GetMissingCellsInRange(void * instance, struct Vector3 min, struct Vector3 max, int bufferSize, void * buffer) {
	printf("invoke HkUniformGridShape_GetMissingCellsInRange\n");
	return __PVE_HkUniformGridShape_GetMissingCellsInRange(instance, min, max, bufferSize, buffer);
}

int (*__PVE_HkUniformGridShape_InvalidateRange)(void * instance, struct Vector3 min, struct Vector3 max, int bufferSize, void * buffer) __attribute__((ms_abi));
int HkUniformGridShape_InvalidateRange(void * instance, struct Vector3 min, struct Vector3 max, int bufferSize, void * buffer) {
	printf("invoke HkUniformGridShape_InvalidateRange\n");
	return __PVE_HkUniformGridShape_InvalidateRange(instance, min, max, bufferSize, buffer);
}

void (*__PVE_HkUniformGridShape_InvalidateRangeImmediate)(void * instance, struct Vector3I minChanged, struct Vector3I maxChanged) __attribute__((ms_abi));
void HkUniformGridShape_InvalidateRangeImmediate(void * instance, struct Vector3I minChanged, struct Vector3I maxChanged) {
	printf("invoke HkUniformGridShape_InvalidateRangeImmediate\n");
	return __PVE_HkUniformGridShape_InvalidateRangeImmediate(instance, minChanged, maxChanged);
}

void (*__PVE_HkUniformGridShape_RemoveChild)(void * instance, int x, int y, int z) __attribute__((ms_abi));
void HkUniformGridShape_RemoveChild(void * instance, int x, int y, int z) {
	printf("invoke HkUniformGridShape_RemoveChild\n");
	return __PVE_HkUniformGridShape_RemoveChild(instance, x, y, z);
}

void (*__PVE_HkUniformGridShape_SetChild)(void * instance, int x, int y, int z, void * shape, int refPolicy) __attribute__((ms_abi));
void HkUniformGridShape_SetChild(void * instance, int x, int y, int z, void * shape, int refPolicy) {
	printf("invoke HkUniformGridShape_SetChild\n");
	return __PVE_HkUniformGridShape_SetChild(instance, x, y, z, shape, refPolicy);
}

void * (*__PVE_HkUniformGridShape_GetChild)(void * instance, int x, int y, int z) __attribute__((ms_abi));
void * HkUniformGridShape_GetChild(void * instance, int x, int y, int z) {
	printf("invoke HkUniformGridShape_GetChild\n");
	return __PVE_HkUniformGridShape_GetChild(instance, x, y, z);
}

void (*__PVE_HkUniformGridShape_SetDeleteHandler)(void * instance, void * handler) __attribute__((ms_abi));
void HkUniformGridShape_SetDeleteHandler(void * instance, void * handler) {
	printf("invoke HkUniformGridShape_SetDeleteHandler\n");
	return __PVE_HkUniformGridShape_SetDeleteHandler(instance, _PVE_Trampoline_Havok_HkDeleteHandler(handler));
}

void (*__PVE_HkUniformGridShape_RemoveShapeRequestHandler)(void * instance) __attribute__((ms_abi));
void HkUniformGridShape_RemoveShapeRequestHandler(void * instance) {
	printf("invoke HkUniformGridShape_RemoveShapeRequestHandler\n");
	return __PVE_HkUniformGridShape_RemoveShapeRequestHandler(instance);
}

void (*__PVE_HkUniformGridShape_SetShapeRequestHandler)(void * instance, void * blockingCallback) __attribute__((ms_abi));
void HkUniformGridShape_SetShapeRequestHandler(void * instance, void * blockingCallback) {
	printf("invoke HkUniformGridShape_SetShapeRequestHandler\n");
	return __PVE_HkUniformGridShape_SetShapeRequestHandler(instance, _PVE_Trampoline_Havok_HkUniformGridShape_NativeBatchRequestCallback(blockingCallback));
}

void (*__PVE_HkUniformGridShape_EnableExtendedCache)(void * instance) __attribute__((ms_abi));
void HkUniformGridShape_EnableExtendedCache(void * instance) {
	printf("invoke HkUniformGridShape_EnableExtendedCache\n");
	return __PVE_HkUniformGridShape_EnableExtendedCache(instance);
}

int (*__PVE_HkConstraintStabilizationUtil_StabilizeRagdollInertias)(void * physicsSystem, float stabilizationAmount, float solverStabilizationAmount) __attribute__((ms_abi));
int HkConstraintStabilizationUtil_StabilizeRagdollInertias(void * physicsSystem, float stabilizationAmount, float solverStabilizationAmount) {
	printf("invoke HkConstraintStabilizationUtil_StabilizeRagdollInertias\n");
	return __PVE_HkConstraintStabilizationUtil_StabilizeRagdollInertias(physicsSystem, stabilizationAmount, solverStabilizationAmount);
}

void (*__PVE_HkDestructionUtils_FindAllBreakableShapesIntersectingSphere)(void * destructionWorld, void * breakableBody, struct Quaternion breakableBodyRotation, struct Vector3 breakableBodyPosition, struct Vector3 position, float radius, void * returnBreakableShape) __attribute__((ms_abi));
void HkDestructionUtils_FindAllBreakableShapesIntersectingSphere(void * destructionWorld, void * breakableBody, struct Quaternion breakableBodyRotation, struct Vector3 breakableBodyPosition, struct Vector3 position, float radius, void * returnBreakableShape) {
	printf("invoke HkDestructionUtils_FindAllBreakableShapesIntersectingSphere\n");
	return __PVE_HkDestructionUtils_FindAllBreakableShapesIntersectingSphere(destructionWorld, breakableBody, breakableBodyRotation, breakableBodyPosition, position, radius, _PVE_Trampoline_Havok_HkDestructionUtils_ReturnBreakableShape(returnBreakableShape));
}

void (*__PVE_HkKeyFrameUtility_ApplyHardKeyFrame)(struct Vector4 nextPosition, struct Quaternion nextOrientation, float invDeltaTime, void * body) __attribute__((ms_abi));
void HkKeyFrameUtility_ApplyHardKeyFrame(struct Vector4 nextPosition, struct Quaternion nextOrientation, float invDeltaTime, void * body) {
	printf("invoke HkKeyFrameUtility_ApplyHardKeyFrame\n");
	return __PVE_HkKeyFrameUtility_ApplyHardKeyFrame(nextPosition, nextOrientation, invDeltaTime, body);
}

void * (*__PVE_HkMassChangerUtil_Create)(void * body, int otherBodyLayerMask, float invMassScale, float invMassScaleOtherBody) __attribute__((ms_abi));
void * HkMassChangerUtil_Create(void * body, int otherBodyLayerMask, float invMassScale, float invMassScaleOtherBody) {
	printf("invoke HkMassChangerUtil_Create\n");
	return __PVE_HkMassChangerUtil_Create(body, otherBodyLayerMask, invMassScale, invMassScaleOtherBody);
}

int (*__PVE_HkMassChangerUtil_IsValid)(void * listener) __attribute__((ms_abi));
int HkMassChangerUtil_IsValid(void * listener) {
	printf("invoke HkMassChangerUtil_IsValid\n");
	return __PVE_HkMassChangerUtil_IsValid(listener);
}

void (*__PVE_HkMassChangerUtil_Remove)(void * listener) __attribute__((ms_abi));
void HkMassChangerUtil_Remove(void * listener) {
	printf("invoke HkMassChangerUtil_Remove\n");
	return __PVE_HkMassChangerUtil_Remove(listener);
}

float (*__PVE_HkUtils_CalculateSeparatingVelocity)(void * body1, void * body2, void * contactPoint) __attribute__((ms_abi));
float HkUtils_CalculateSeparatingVelocity(void * body1, void * body2, void * contactPoint) {
	printf("invoke HkUtils_CalculateSeparatingVelocity\n");
	return __PVE_HkUtils_CalculateSeparatingVelocity(body1, body2, contactPoint);
}

void (*__PVE_HkUtils_SetSoftContact)(void * bodyA, void * bodyB, float softness, float maxVel) __attribute__((ms_abi));
void HkUtils_SetSoftContact(void * bodyA, void * bodyB, float softness, float maxVel) {
	printf("invoke HkUtils_SetSoftContact\n");
	return __PVE_HkUtils_SetSoftContact(bodyA, bodyB, softness, maxVel);
}

void (*__PVE_HkIntermediateBuffer_ReleaseUnmanaged)(void * memory) __attribute__((ms_abi));
void HkIntermediateBuffer_ReleaseUnmanaged(void * memory) {
	printf("invoke HkIntermediateBuffer_ReleaseUnmanaged\n");
	return __PVE_HkIntermediateBuffer_ReleaseUnmanaged(memory);
}

char* __havok_PVEExports[] = {
	"HkCharacterProxy_Create",
	"HkCharacterProxy_GetPosition",
	"HkCharacterProxy_SetPosition",
	"HkCharacterProxy_GetState",
	"HkCharacterProxy_SetState",
	"HkCharacterProxy_StepSimulation",
	"HkCharacterProxy_GetLinearVelocity",
	"HkCharacterProxy_SetLinearVelocity",
	"HkCharacterProxy_SetUp",
	"HkCharacterProxyCinfo_Create",
	"HkCharacterProxyCinfo_GetPosition",
	"HkCharacterProxyCinfo_SetPosition",
	"HkCharacterProxyCinfo_GetVelocity",
	"HkCharacterProxyCinfo_SetVelocity",
	"HkCharacterProxyCinfo_GetDynamicFriction",
	"HkCharacterProxyCinfo_SetDynamicFriction",
	"HkCharacterProxyCinfo_GetStaticFriction",
	"HkCharacterProxyCinfo_SetStaticFriction",
	"HkCharacterProxyCinfo_GetKeepContactTolerance",
	"HkCharacterProxyCinfo_SetKeepContactTolerance",
	"HkCharacterProxyCinfo_GetUp",
	"HkCharacterProxyCinfo_SetUp",
	"HkCharacterProxyCinfo_GetExtraUpStaticFriction",
	"HkCharacterProxyCinfo_SetExtraUpStaticFriction",
	"HkCharacterProxyCinfo_GetExtraDownStaticFriction",
	"HkCharacterProxyCinfo_SetExtraDownStaticFriction",
	"HkCharacterProxyCinfo_SetShapePhantom",
	"HkCharacterProxyCinfo_GetShapePhantom",
	"HkCharacterProxyCinfo_GetKeepDistance",
	"HkCharacterProxyCinfo_SetKeepDistance",
	"HkCharacterProxyCinfo_GetContactAngleSensitivity",
	"HkCharacterProxyCinfo_SetContactAngleSensitivity",
	"HkCharacterProxyCinfo_GetUserPlanes",
	"HkCharacterProxyCinfo_SetUserPlanes",
	"HkCharacterProxyCinfo_GetMaxCharacterSpeedForSolver",
	"HkCharacterProxyCinfo_SetMaxCharacterSpeedForSolver",
	"HkCharacterProxyCinfo_GetCharacterStrength",
	"HkCharacterProxyCinfo_SetCharacterStrength",
	"HkCharacterProxyCinfo_GetCharacterMass",
	"HkCharacterProxyCinfo_SetCharacterMass",
	"HkCharacterProxyCinfo_GetMaxSlope",
	"HkCharacterProxyCinfo_SetMaxSlope",
	"HkCharacterProxyCinfo_GetPenetrationRecoverySpeed",
	"HkCharacterProxyCinfo_SetPenetrationRecoverySpeed",
	"HkCharacterProxyCinfo_GetMaxCastIterations",
	"HkCharacterProxyCinfo_SetMaxCastIterations",
	"HkCharacterProxyCinfo_GetRefreshManifoldInCheckSupport",
	"HkCharacterProxyCinfo_SetRefreshManifoldInCheckSupport",
	"HkCharacterRigidBody_Create",
	"HkCharacterRigidBody_GetCharacterRigidbody",
	"HkCharacterRigidBody_SetWalkingState",
	"HkCharacterRigidBody_SetFlyingState",
	"HkCharacterRigidBody_SetLadderState",
	"HkCharacterRigidBody_SetDefaultShape",
	"HkCharacterRigidBody_SetShapeForCrouch",
	"HkCharacterRigidBody_GetPosition",
	"HkCharacterRigidBody_SetPosition",
	"HkCharacterRigidBody_GetState",
	"HkCharacterRigidBody_SetState",
	"HkCharacterRigidBody_StepSimulation",
	"HkCharacterRigidBody_UpdateVelocity",
	"HkCharacterRigidBody_UpdateSupport",
	"HkCharacterRigidBody_SetRigidBodyTransform",
	"HkCharacterRigidBody_GetRigidBodyTransform",
	"HkCharacterRigidBody_GetLinearVelocity",
	"HkCharacterRigidBody_SetLinearVelocity",
	"HkCharacterRigidBody_ApplyLinearImpulse",
	"HkCharacterRigidBody_ApplyAngularImpulse",
	"HkCharacterRigidBody_SetSupportDistance",
	"HkCharacterRigidBody_SetHardSupportDistance",
	"HkCharacterRigidBody_GetAngularVelocity",
	"HkCharacterRigidBody_SetAngularVelocity",
	"HkCharacterRigidBody_IsSupportedByFloatingObject",
	"HkCharacterRigidBody_IsSupported",
	"HkCharacterRigidBody_GetSupportNormal",
	"HkCharacterRigidBody_GetGroundVelocity",
	"HkCharacterRigidBody_GetUseSupportInfoQuery",
	"HkCharacterRigidBody_SetUseSupportInfoQuery",
	"HkCharacterRigidBody_SetPreviousSupportedState",
	"HkCharacterRigidBody_ResetSurfaceVelocity",
	"HkCharacterRigidBody_SetMaxSlope",
	"HkCharacterRigidBody_GetMaxSlope",
	"HkCharacterRigidBody_GetSupportBodies",
	"HkCharacterRigidBodyCinfo_Create",
	"HkCharacterRigidBodyCinfo_GetCollisionFilterInfo",
	"HkCharacterRigidBodyCinfo_SetCollisionFilterInfo",
	"HkCharacterRigidBodyCinfo_GetShape",
	"HkCharacterRigidBodyCinfo_SetShape",
	"HkCharacterRigidBodyCinfo_GetPosition",
	"HkCharacterRigidBodyCinfo_SetPosition",
	"HkCharacterRigidBodyCinfo_GetRotation",
	"HkCharacterRigidBodyCinfo_SetRotation",
	"HkCharacterRigidBodyCinfo_GetMass",
	"HkCharacterRigidBodyCinfo_SetMass",
	"HkCharacterRigidBodyCinfo_GetFriction",
	"HkCharacterRigidBodyCinfo_SetFriction",
	"HkCharacterRigidBodyCinfo_GetMaxLinearVelocity",
	"HkCharacterRigidBodyCinfo_SetMaxLinearVelocity",
	"HkCharacterRigidBodyCinfo_GetAllowedPenetrationDepth",
	"HkCharacterRigidBodyCinfo_SetAllowedPenetrationDepth",
	"HkCharacterRigidBodyCinfo_GetUp",
	"HkCharacterRigidBodyCinfo_SetUp",
	"HkCharacterRigidBodyCinfo_GetMaxSlope",
	"HkCharacterRigidBodyCinfo_SetMaxSlope",
	"HkCharacterRigidBodyCinfo_GetMaxForce",
	"HkCharacterRigidBodyCinfo_SetMaxForce",
	"HkCharacterRigidBodyCinfo_GetUnweldingHeightOffsetFactor",
	"HkCharacterRigidBodyCinfo_SetUnweldingHeightOffsetFactor",
	"HkCharacterRigidBodyCinfo_GetMaxSpeedForSimplexSolver",
	"HkCharacterRigidBodyCinfo_SetMaxSpeedForSimplexSolver",
	"HkCharacterRigidBodyCinfo_GetSupportDistance",
	"HkCharacterRigidBodyCinfo_SetSupportDistance",
	"HkCharacterRigidBodyCinfo_GetHardSupportDistance",
	"HkCharacterRigidBodyCinfo_SetHardSupportDistance",
	"HkBallAndSocketConstraintData_Create",
	"HkBallAndSocketConstraintData_SetInBodySpaceInternal",
	"HkBreakableConstraintData_Create",
	"HkBreakableConstraintData_GetThreshold",
	"HkBreakableConstraintData_SetThreshold",
	"HkBreakableConstraintData_GetRemoveFromWorldOnBrake",
	"HkBreakableConstraintData_SetRemoveFromWorldOnBrake",
	"HkBreakableConstraintData_GetReapplyVelocityOnBreak",
	"HkBreakableConstraintData_SetReapplyVelocityOnBreak",
	"HkBreakableConstraintData_GetIsBroken",
	"HkCogWheelConstraintData_Create",
	"HkCogWheelConstraintData_SetInWorldSpace",
	"HkCogWheelConstraintData_SetInBodySpaceInternal",
	"HkConstraint_Create",
	"HkConstraint_AddConstraintListener",
	"HkConstraint_RemoveConstraintListener",
	"HkConstraint_ReplaceEntity",
	"HkConstraint_SetVirtualMassInverse",
	"HkConstraint_GetPriority",
	"HkConstraint_SetPriority",
	"HkConstraint_GetWantRuntime",
	"HkConstraint_SetWantRuntime",
	"HkConstraint_IsInWorld",
	"HkConstraint_GetRigidBodyA",
	"HkConstraint_GetRigidBodyB",
	"HkConstraint_GetEnabled",
	"HkConstraint_SetEnabled",
	"HkConstraint_GetPivotsInWorld",
	"HkConstraint_GetUserData",
	"HkConstraint_SetUserData",
	"HkConstraint_AddCenterOfMassModifierAtom",
	"HkConstraint_FindConnectedConstraints",
	"HkConstraintData_GetMaximumLinearImpulse",
	"HkConstraintData_SetMaximumLinearImpulse",
	"HkConstraintData_GetMaximumAngularImpulse",
	"HkConstraintData_SetMaximumAngularImpulse",
	"HkConstraintData_GetBreachImpulse",
	"HkConstraintData_SetBreachImpulse",
	"HkConstraintData_GetInertiaStabilizationFactor",
	"HkConstraintData_SetInertiaStabilizationFactor",
	"HkConstraintData_SetSolvingMethod",
	"HkConstraintListener_Create",
	"HkConstraintListener_Release",
	"HkConstraintListener_SetCallbacks",
	"HkCustomWheelConstraintData_Create",
	"HkCustomWheelConstraintData_GetLimitsEnabled",
	"HkCustomWheelConstraintData_SetLimitsEnabled",
	"HkCustomWheelConstraintData_GetSuspensionMinLimit",
	"HkCustomWheelConstraintData_SetSuspensionMinLimit",
	"HkCustomWheelConstraintData_GetSuspensionMaxLimit",
	"HkCustomWheelConstraintData_SetSuspensionMaxLimit",
	"HkCustomWheelConstraintData_GetFrictionEnabled",
	"HkCustomWheelConstraintData_SetFrictionEnabled",
	"HkCustomWheelConstraintData_GetMaxFrictionTorque",
	"HkCustomWheelConstraintData_SetMaxFrictionTorque",
	"HkCustomWheelConstraintData_SetInBodySpaceInternal",
	"HkCustomWheelConstraintData_SetSuspensionStrength",
	"HkCustomWheelConstraintData_SetSuspensionDamping",
	"HkCustomWheelConstraintData_SetSteeringAngle",
	"HkCustomWheelConstraintData_SetAngleLimits",
	"HkCustomWheelConstraintData_GetAngleLimitsMin",
	"HkCustomWheelConstraintData_GetAngleLimitsMax",
	"HkCustomWheelConstraintData_DisableLimits",
	"HkCustomWheelConstraintData_GetCurrentAngle",
	"HkCustomWheelConstraintData_SetCurrentAngle",
	"HkFixedConstraintData_Create",
	"HkFixedConstraintData_SetInBodySpaceInternal",
	"HkFixedConstraintData_SetInWorldSpace",
	"HkFixedConstraintData_IsValid",
	"HkFixedConstraintData_SetInertiaStabilizationFactor",
	"HkFixedConstraintData_GetInertiaStabilizationFactor",
	"HkFixedConstraintData_GetSolverImpulseInLastStep",
	"HkHingeConstraintData_Create",
	"HkHingeConstraintData_SetInBodySpaceInternal",
	"HkHingeConstraintData_SetInWorldSpace",
	"HkHingeConstraintData_SetInertiaStabilizationFactor",
	"HkHingeConstraintData_GetInertiaStabilizationFactor",
	"HkLimitedForceConstraintMotor_GetMinForce",
	"HkLimitedForceConstraintMotor_SetMinForce",
	"HkLimitedForceConstraintMotor_GetMaxForce",
	"HkLimitedForceConstraintMotor_SetMaxForce",
	"HkLimitedHingeConstraintData_Create",
	"HkLimitedHingeConstraintData_SetInBodySpaceInternal",
	"HkLimitedHingeConstraintData_SetInWorldSpace",
	"HkLimitedHingeConstraintData_SetMotor",
	"HkLimitedHingeConstraintData_IsMotorEnabled",
	"HkLimitedHingeConstraintData_SetMotorEnabled",
	"HkLimitedHingeConstraintData_GetTargetAngle",
	"HkLimitedHingeConstraintData_SetTargetAngle",
	"HkLimitedHingeConstraintData_GetMaxFrictionTorque",
	"HkLimitedHingeConstraintData_SetMaxFrictionTorque",
	"HkLimitedHingeConstraintData_GetMinAngularLimit",
	"HkLimitedHingeConstraintData_SetMinAngularLimit",
	"HkLimitedHingeConstraintData_GetMaxAngularLimit",
	"HkLimitedHingeConstraintData_SetMaxAngularLimit",
	"HkLimitedHingeConstraintData_DisableLimits",
	"HkLimitedHingeConstraintData_SetInertiaStabilizationFactor",
	"HkLimitedHingeConstraintData_GetInertiaStabilizationFactor",
	"HkLimitedHingeConstraintData_GetBodyAPos",
	"HkLimitedHingeConstraintData_GetBodyBPos",
	"HkLimitedHingeConstraintData_GetIsInitialized",
	"HkLimitedHingeConstraintData_SetIsInitialized",
	"HkLimitedHingeConstraintData_GetPreviousTargetAngle",
	"HkLimitedHingeConstraintData_SetPreviousTargetAngle",
	"HkLimitedHingeConstraintData_GetCurrentAngle",
	"HkLimitedHingeConstraintData_SetCurrentAngle",
	"HkMalleableConstraintData_Create",
	"HkMalleableConstraintData_GetStrength",
	"HkMalleableConstraintData_SetStrength",
	"HkPrismaticConstraintData_Create",
	"HkPrismaticConstraintData_SetInWorldSpace",
	"HkPrismaticConstraintData_SetInBodySpaceInternal",
	"HkPrismaticConstraintData_GetMaximumLinearLimit",
	"HkPrismaticConstraintData_SetMaximumLinearLimit",
	"HkPrismaticConstraintData_GetMinimumLinearLimit",
	"HkPrismaticConstraintData_SetMinimumLinearLimit",
	"HkPrismaticConstraintData_GetMaxFrictionForce",
	"HkPrismaticConstraintData_SetMaxFrictionForce",
	"HkPrismaticConstraintData_GetTargetPosition",
	"HkPrismaticConstraintData_SetTargetPosition",
	"HkPrismaticConstraintData_SetMotor",
	"HkPrismaticConstraintData_IsMotorEnabled",
	"HkPrismaticConstraintData_SetMotorEnabled",
	"HkPrismaticConstraintData_GetCurrentPosition",
	"HkRopeConstraintData_Create",
	"HkRopeConstraintData_SetInBodySpaceInternal",
	"HkRopeConstraintData_Update",
	"HkRopeConstraintData_GetStrength",
	"HkRopeConstraintData_SetStrength",
	"HkRopeConstraintData_GetLinearLimit",
	"HkRopeConstraintData_SetLinearLimit",
	"HkRopeConstraintData_IsValid",
	"HkVelocityConstraintMotor_Create",
	"HkVelocityConstraintMotor_GetTau",
	"HkVelocityConstraintMotor_SetTau",
	"HkVelocityConstraintMotor_GetVelocityTarget",
	"HkVelocityConstraintMotor_SetVelocityTarget",
	"HkVelocityConstraintMotor_GetConstantRecoveryVelocity",
	"HkVelocityConstraintMotor_SetConstantRecoveryVelocity",
	"HkWheelConstraintData_Create",
	"HkWheelConstraintData_SetInWorldSpace",
	"HkWheelConstraintData_SetInBodySpaceInternal",
	"HkWheelConstraintData_SetSuspensionMinLimit",
	"HkWheelConstraintData_SetSuspensionMaxLimit",
	"HkWheelConstraintData_SetSuspensionStrength",
	"HkWheelConstraintData_SetSuspensionDamping",
	"HkWheelConstraintData_SetSteeringAngle",
	"HkdDecomposeFracture_Create",
	"HkdDecomposeFracture_GetClipZoneWidth",
	"HkdDecomposeFracture_SetClipZoneWidth",
	"HkdDecomposeFracture_GetShiftToSmallerCrossSection",
	"HkdDecomposeFracture_SetShiftToSmallerCrossSection",
	"HkdDecomposeFracture_SetGeometry",
	"HkdFracture_GetFlattenHierarchy",
	"HkdFracture_SetFlattenHierarchy",
	"HkdFracture_GetRefitType",
	"HkdFracture_SetRefitType",
	"HkdRandomSplitFracture_Create",
	"HkdRandomSplitFracture_ReCast",
	"HkdRandomSplitFracture_GetRandomRange",
	"HkdRandomSplitFracture_SetRandomRange",
	"HkdRandomSplitFracture_GetSplitGeometryScale",
	"HkdRandomSplitFracture_SetSplitGeometryScale",
	"HkdRandomSplitFracture_GetSplitLargestVolumesFirst",
	"HkdRandomSplitFracture_SetSplitLargestVolumesFirst",
	"HkdRandomSplitFracture_GetRandomSeed",
	"HkdRandomSplitFracture_SetRandomSeed",
	"HkdRandomSplitFracture_GetNumObjectsOnLevel",
	"HkdRandomSplitFracture_SetNumObjectsOnLevel",
	"HkdRandomSplitFracture_SetGeometry",
	"HkdVoronoiFracture_Create",
	"HkdVoronoiFracture_GetNumIterations",
	"HkdVoronoiFracture_SetNumIterations",
	"HkdVoronoiFracture_GetNumSitesToGenerate",
	"HkdVoronoiFracture_SetNumSitesToGenerate",
	"HkdVoronoiFracture_GetSeed",
	"HkdVoronoiFracture_SetSeed",
	"HkdVoronoiFracture_SetGeometry",
	"HkdWoodFracture_Create",
	"HkdWoodFracture_ReCast",
	"HkdWoodFracture_GetSplinterSplittingGeometry",
	"HkdWoodFracture_SetSplinterSplittingGeometry",
	"HkdWoodFracture_GetBoardSplittingGeometry",
	"HkdWoodFracture_SetBoardSplittingGeometry",
	"HkdWoodFracture_GetSplinterSplittingData",
	"HkdWoodFracture_SetSplinterSplittingData",
	"HkdWoodFracture_GetBoardSplittingData",
	"HkdWoodFracture_SetBoardSplittingData",
	"HkdWoodFracture_GetRandomSeed",
	"HkdWoodFracture_SetRandomSeed",
	"HkBreakOffPartsUtil_Create",
	"HkBreakOffPartsUtil_Release",
	"HkBreakOffPartsUtil_RemoveKeysFromListShape",
	"HkBreakOffPartsUtil_MarkEntityBreakable",
	"HkBreakOffPartsUtil_MarkPieceBreakable",
	"HkBreakOffPartsUtil_SetMaxConstraintImpulse",
	"HkBreakOffPartsUtil_UnmarkEntityBreakable",
	"HkBreakOffPartsUtil_UnmarkPieceBreakable",
	"HkBreakOffPoints_Count",
	"HkBreakOffPoints_Get",
	"HkdBreakableBody_Create",
	"HkdBreakableBody_InitListener",
	"HkdBreakableBody_GetBreakableShape",
	"HkdBreakableBody_SetBreakableShape",
	"HkdBreakableBody_Clear",
	"HkdBreakableBody_ConnectToWorld",
	"HkdBreakableBody_GetRigidBody",
	"HkdBreakableBody_Initialize",
	"HkdBreakableBody_RemoveConnection",
	"HkdBreakableBody_SetFixedConnectivity",
	"HkdBreakableBodyHelper_GetChildren",
	"HkdBreakableBodyHelper_GetRigidBodyMatrix",
	"HkdBreakableBodyHelper_GetShapeCoM",
	"HkdBreakableBodyInfo_GetBody",
	"HkdBreakableBodyInfo_IsFracture",
	"HkdBreakableShape_Create",
	"HkdBreakableShape_CreateWithMass",
	"HkdBreakableShape_GetShapeName",
	"HkdBreakableShape_GetMaterialType",
	"HkdBreakableShape_GetMotionQuality",
	"HkdBreakableShape_SetMotionQuality",
	"HkdBreakableShape_GetHasParent",
	"HkdBreakableShape_GetName",
	"HkdBreakableShape_SetName",
	"HkdBreakableShape_GetVolume",
	"HkdBreakableShape_SetVolume",
	"HkdBreakableShape_GetUserObject",
	"HkdBreakableShape_SetUserObject",
	"HkdBreakableShape_GetCoM",
	"HkdBreakableShape_GetReferenceCount",
	"HkdBreakableShape_SetReferenceCount",
	"HkdBreakableShape_CopyData",
	"HkdBreakableShape_DisposeSharedMaterial",
	"HkdBreakableShape_AddConnection",
	"HkdBreakableShape_AddReference",
	"HkdBreakableShape_AddShape",
	"HkdBreakableShape_AutoConnect",
	"HkdBreakableShape_BuildMassProperties",
	"HkdBreakableShape_CalculateGeometryVolume",
	"HkdBreakableShape_ClearActions",
	"HkdBreakableShape_ClearConnections",
	"HkdBreakableShape_ClearConnectionsRecursive",
	"HkdBreakableShape_ClearHandle",
	"HkdBreakableShape_Clone",
	"HkdBreakableShape_ConnectSemiAccurate",
	"HkdBreakableShape_DisableRefCount",
	"HkdBreakableShape_DisableRefCountRecursively",
	"HkdBreakableShape_GetChild",
	"HkdBreakableShape_GetChildren",
	"HkdBreakableShape_GetChildrenCount",
	"HkdBreakableShape_GetChildShape",
	"HkdBreakableShape_GetConnectionList",
	"HkdBreakableShape_GetMass",
	"HkdBreakableShape_GetParent",
	"HkdBreakableShape_GetProperty",
	"HkdBreakableShape_GetShape",
	"HkdBreakableShape_GetStrenght",
	"HkdBreakableShape_GetTotalChildrenCount",
	"HkdBreakableShape_HasFixedChildren",
	"HkdBreakableShape_HasProperty",
	"HkdBreakableShape_InitIntegrity",
	"HkdBreakableShape_IsChildOf",
	"HkdBreakableShape_IsCompound",
	"HkdBreakableShape_IsDescendantOf",
	"HkdBreakableShape_IsFixed",
	"HkdBreakableShape_IsFracturePiece",
	"HkdBreakableShape_IsValid",
	"HkdBreakableShape_RemoveChild",
	"HkdBreakableShape_RemoveChildByName",
	"HkdBreakableShape_RemoveConnection",
	"HkdBreakableShape_RemoveReference",
	"HkdBreakableShape_ReplaceChildren",
	"HkdBreakableShape_ReplaceConnections",
	"HkdBreakableShape_SetAsDebris",
	"HkdBreakableShape_SetAsDebrisRecursive",
	"HkdBreakableShape_SetAsFixed",
	"HkdBreakableShape_SetChildrenParent",
	"HkdBreakableShape_SetFlagRecursively",
	"HkdBreakableShape_SetHasFixedChildren",
	"HkdBreakableShape_SetMass",
	"HkdBreakableShape_SetMassProperties",
	"HkdBreakableShape_SetMassRecursively",
	"HkdBreakableShape_SetMotionQualityRecursively",
	"HkdBreakableShape_SetProperty",
	"HkdBreakableShape_SetPropertyRecursively",
	"HkdBreakableShape_SetStrenght",
	"HkdBreakableShape_SetStrenghtRecursively",
	"HkdCompoundBreakableShape_Create",
	"HkdCompoundBreakableShape_DisableChild",
	"HkdCompoundBreakableShape_RecalcMassPropsFromChildren",
	"HkdConnection_Create",
	"HkdConnection_CreateWithParams",
	"HkdConnection_GetShapeB",
	"HkdConnection_SetShapeB",
	"HkdConnection_GetShapeA",
	"HkdConnection_SetShapeA",
	"HkdConnection_GetShapeBName",
	"HkdConnection_GetShapeAName",
	"HkdConnection_GetContactArea",
	"HkdConnection_SetContactArea",
	"HkdConnection_GetSeparatingNormal",
	"HkdConnection_SetSeparatingNormal",
	"HkdConnection_GetPivotB",
	"HkdConnection_SetPivotB",
	"HkdConnection_GetPivotA",
	"HkdConnection_SetPivotA",
	"HkdConnection_AddToCommonParent",
	"HkdConnection_IsValid",
	"HkdConnection_RemoveReference",
	"HkdFixedConnectivity_Create",
	"HkdFixedConnectivity_AddConnection",
	"HkdFixedConnectivity_RemoveReference",
	"HkdFixedConnectivity_CreateConnection",
	"HkdFractureImpactDetails_Create",
	"HkdFractureImpactDetails_GetFlag",
	"HkdFractureImpactDetails_SetFlag",
	"HkdFractureImpactDetails_GetBreakingBody",
	"HkdFractureImpactDetails_GetContactPoint",
	"HkdFractureImpactDetails_IsValid",
	"HkdFractureImpactDetails_RemoveReference",
	"HkdFractureImpactDetails_SetBreakingBody",
	"HkdFractureImpactDetails_SetBreakingImpulse",
	"HkdFractureImpactDetails_SetContactPoint",
	"HkdFractureImpactDetails_SetDestructionRadius",
	"HkdFractureImpactDetails_SetOtherBody",
	"HkdFractureImpactDetails_SetParticleExpandVelocity",
	"HkdFractureImpactDetails_SetParticleMass",
	"HkdFractureImpactDetails_SetParticlePosition",
	"HkdFractureImpactDetails_SetParticleVelocity",
	"HkdFractureImpactDetails_ZeroCollidingParticleVelocity",
	"HkdReplaceBodyEvent_GetOldBody",
	"HkdReplaceBodyEvent_GetNewBodies",
	"HkdShapeInstanceInfo_Create",
	"HkdShapeInstanceInfo_CreateWithTranslation",
	"HkdShapeInstanceInfo_Release",
	"HkdShapeInstanceInfo_GetDynamicParent",
	"HkdShapeInstanceInfo_SetDynamicParent",
	"HkdShapeInstanceInfo_GetShape",
	"HkdShapeInstanceInfo_GetShapeName",
	"HkdShapeInstanceInfo_GetCoM",
	"HkdShapeInstanceInfo_GetChildren",
	"HkdShapeInstanceInfo_GetTransform",
	"HkdShapeInstanceInfo_InstanceOf",
	"HkdShapeInstanceInfo_IsFracturePiece",
	"HkdShapeInstanceInfo_IsReferenceValid",
	"HkdShapeInstanceInfo_IsValid",
	"HkdShapeInstanceInfo_RemoveReference",
	"HkdShapeInstanceInfo_RemoveReferenceFromShape",
	"HkdShapeInstanceInfo_SetTransform",
	"HkdWorld_Create",
	"HkdWorld_AddBreakableBody",
	"HkdWorld_RemoveBreakableBodyWithInfo",
	"HkdWorld_RemoveBreakableBody",
	"HkdWorld_TriggerDestruction",
	"HkdWorld_Release",
	"HkDestructionStorage_Create",
	"HkDestructionStorage_CleanChildrenShapes",
	"HkDestructionStorage_CreateGeometry",
	"HkDestructionStorage_MakeShapeFromData",
	"HkDestructionStorage_DumpDestructionData",
	"HkDestructionStorage_FractureShape",
	"HkDestructionStorage_GetDataFromShape",
	"HkDestructionStorage_GetMaterialsOnRegisteredShapes",
	"HkDestructionStorage_GetRegisteredMaterials",
	"HkDestructionStorage_GetRegisteredShapes",
	"HkDestructionStorage_LoadDestructionDataFromBuffer",
	"HkDestructionStorage_RegisterShape",
	"HkDestructionStorage_RegisterShapeWithGraphics",
	"HkDestructionStorage_SaveDestructionData",
	"HkDestructionStorage_SerializeDestructionData",
	"HkDestructionStorage_ReleasePtr",
	"HkEasePenetrationAction_Create",
	"HkEasePenetrationAction_GetInitialAdditionalAllowedPenetrationDepth",
	"HkEasePenetrationAction_SetInitialAdditionalAllowedPenetrationDepth",
	"HkEasePenetrationAction_GetInitialAllowedPenetrationDepthMultiplier",
	"HkEasePenetrationAction_SetInitialAllowedPenetrationDepthMultiplier",
	"HkGeometry_Create",
	"HkGeometry_CreateWithParams",
	"HkGeometry_Destroy",
	"HkGeometry_GetTriangleCount",
	"HkGeometry_GetVertexCount",
	"HkGeometry_Append",
	"HkGeometry_GetTriangle",
	"HkGeometry_GetVertex",
	"HkGeometry_SetGeometry",
	"HkGroupFilter_CalcFilterInfo",
	"HkGroupFilter_GetLayerFromFilterInfo",
	"HkGroupFilter_getSubSystemDontCollideWithFromFilterInfo",
	"HkGroupFilter_GetSubSystemIdFromFilterInfo",
	"HkGroupFilter_GetSystemGroupFromFilterInfo",
	"HkGroupFilter_SetLayer",
	"HkGroupFilter_DisableCollisionsBetween",
	"HkGroupFilter_DisableCollisionsUsingBitfield",
	"HkGroupFilter_EnableCollisionsBetween",
	"HkGroupFilter_EnableCollisionsUsingBitfield",
	"HkGroupFilter_GetNewSystemGroup",
	"HkInertiaTensorComputer_Create",
	"HkInertiaTensorComputer_CombineMassPropertiesInstance",
	"HkInertiaTensorComputer_Release",
	"HkInertiaTensorComputer_ComputeBoxVolumeMassProperties",
	"HkInertiaTensorComputer_ComputeCapsuleVolumeMassProperties",
	"HkInertiaTensorComputer_ComputeCylinderVolumeMassProperties",
	"HkInertiaTensorComputer_ComputeSphereVolumeMassProperties",
	"HkMemorySnapshot_Diff",
	"HkShapeCutterUtil_Cut",
	"HkSimpleValueProperty_CreateFloat",
	"HkSimpleValueProperty_CreateUInt",
	"HkSimpleValueProperty_CreateInt",
	"HkSimpleValueProperty_GetValueFloat",
	"HkSimpleValueProperty_SetValueFloat",
	"HkSimpleValueProperty_GetValueUInt",
	"HkSimpleValueProperty_SetValueUInt",
	"HkSimpleValueProperty_GetValueInt",
	"HkSimpleValueProperty_SetValueInt",
	"HkVec3IProperty_Create",
	"HkVec3IProperty_GetValue",
	"HkVec3IProperty_SetValue",
	"HkWheelResponseModifierUtil_Create",
	"HkWheelResponseModifierUtil_Release",
	"HkActivationListener_Create",
	"HkBaseSystem_Init",
	"HkBaseSystem_Quit",
	"HkBaseSystem_InitThread",
	"HkBaseSystem_QuitThread",
	"HkBaseSystem_GetVersionInfo",
	"HkBaseSystem_GetMemoryStatistics",
	"HkBaseSystem_EnableAssert",
	"HkBaseSystem_IsEnabled",
	"HkBaseSystem_IsDestructionEnabled",
	"HkBaseSystem_OnSimulationFrameStarted",
	"HkBaseSystem_OnSimulationFrameFinished",
	"HkBaseSystem_GetKeyCodes",
	"HkBaseSystem_IsOutOfMemory",
	"HkBaseSystem_GetCurrentMemoryConsumption",
	"HkCollisionEvent_GetSource",
	"HkCollisionEvent_GetRigidBody",
	"HkCollisionEvent_GetBodyA",
	"HkCollisionEvent_GetBodyB",
	"HkCollisionEvent_SetImpulse",
	"HkCollisionEvent_SetImpulseScaling",
	"HkCollisionEvent_GetContactPointCount",
	"HkCollisionEvent_Disable",
	"HkCollisionEvent_GetContactPointPropertiesAt",
	"HkCollisionEvent_GetOffsets",
	"HkConstraintProjectorListener_Create",
	"HkConstraintProjectorListener_Release",
	"HkContactListener_Create",
	"HkContactListener_SetCallbackLimit",
	"HkContactListener_ResetLimit",
	"HkContactPoint_GetPosition",
	"HkContactPoint_SetPosition",
	"HkContactPoint_GetNormalAndDistance",
	"HkContactPoint_SetNormalAndDistance",
	"HkContactPoint_GetNormal",
	"HkContactPoint_SetNormal",
	"HkContactPoint_GetDistance",
	"HkContactPoint_SetDistance",
	"HkContactPoint_Flip",
	"HkContactPointEvent_GetBase",
	"HkContactPointEvent_IsToi",
	"HkContactPointEvent_GetSeparatingVelocity",
	"HkContactPointEvent_SetSeparatingVelocity",
	"HkContactPointEvent_GetRotateNormal",
	"HkContactPointEvent_SetRotateNormal",
	"HkContactPointEvent_GetEventType",
	"HkContactPointEvent_GetContactPoint",
	"HkContactPointEvent_GetContactProperties",
	"HkContactPointEvent_GetFiringCallbacksForFullManifold",
	"HkContactPointEvent_GetFirstCallbackForFullManifold",
	"HkContactPointEvent_GetLastCallbackForFullManifold",
	"HkContactPointEvent_GetContactPointId",
	"HkContactPointEvent_AccessVelocities",
	"HkContactPointEvent_UpdateVelocities",
	"HkContactPointEvent_GetShapeKey",
	"HkContactPointEvent_GetShapeKeyWithShapeID",
	"HkContactPointEvent_GetFieldOffsets",
	"HkContactPointProperties_GetImpulseApplied",
	"HkContactPointProperties_GetInternalSolverData",
	"HkContactPointProperties_WasUsed",
	"HkContactPointProperties_GetFriction",
	"HkContactPointProperties_SetFriction",
	"HkContactPointProperties_GetRestitution",
	"HkContactPointProperties_SetRestitution",
	"HkContactPointProperties_IsPotential",
	"HkContactPointProperties_GetMaxImpulsePerStep",
	"HkContactPointProperties_SetMaxImpulsePerStep",
	"HkContactPointProperties_GetMaxImpulse",
	"HkContactPointProperties_SetMaxImpulse",
	"HkContactPointProperties_GetIsDisabled",
	"HkContactPointProperties_SetIsDisabled",
	"HkContactPointProperties_GetIsNew",
	"HkContactPointProperties_SetIsNew",
	"HkContactPointProperties_GetUserData",
	"HkContactPointProperties_SetUserData",
	"HkContactPointProperties_GetFieldOffsets",
	"HkContactSoundListener_Create",
	"HkEntity_AddActivationListener",
	"HkEntity_RemoveActivationListener",
	"HKEntity_AddEntityListener",
	"HKEntity_RemoveEntityListener",
	"HkEntity_SetContactListener",
	"HkEntity_GetQuality",
	"HkEntity_SetQuality",
	"HkEntity_IsFixed",
	"HkEntity_IsFixedOrKeyframed",
	"HkRigidBody_GetMotionType",
	"HkEntity_GetContactPointCallbackDelay",
	"HkEntity_SetContactPointCallbackDelay",
	"HkEntity_SetProperty",
	"HkEntity_HasProperty",
	"HkEntity_RemoveProperty",
	"HkRigidBody_GetRotation",
	"HkRigidBody_SetRotation",
	"HkRigidBody_GetPosition",
	"HkRigidBody_SetPosition",
	"HkRigidBody_Activate",
	"HkRigidBody_ActivateAsCriticalOperation",
	"HkRigidBody_Deactivate",
	"HkRigidBody_UpdateMotionType",
	"HkRigidBody_GetIsActive",
	"HkRigidBody_RequestDeactivation",
	"HkRigidBody_GetLinearVelocity",
	"HkRigidBody_SetLinearVelocity",
	"HkRigidBody_GetAngularVelocity",
	"HkRigidBody_SetAngularVelocity",
	"HkEntity_GetFieldOffsets",
	"HkEntityListener_Create",
	"HkEntityListener_Release",
	"HkGlobal_ReleasePtr",
	"HkGlobal_ReleaseString",
	"HkGlobal_ReleaseArrayPtr",
	"HkJobQueue_Create",
	"HkJobQueue_CreateWithNumThreads",
	"HkJobQueue_Release",
	"HkJobQueue_GetWaitPolicy",
	"HkJobQueue_SetWaitPolicy",
	"HkJobQueue_GetMasterThreadFinishingFlags",
	"HkJobQueue_SetMasterThreadFinishingFlags",
	"HkJobQueue_ProcessAllJobs",
	"HkJobThreadPool_Create",
	"HkJobThreadPool_CreateWithNumThreads",
	"HkJobThreadPool_RemoveReference",
	"HkJobThreadPool_RunOnEachWorker",
	"HkJobThreadPool_ExecuteJobQueue",
	"HkJobThreadPool_GetThisThreadIndex",
	"HkJobThreadPool_WaitForCompletion",
	"HkJobThreadPool_ClearTimerData",
	"HkMotion_SetWorldMatrix",
	"HkMotion_GetDeactivationClass",
	"HkMotion_SetDeactivationClass",
	"HkReferenceObject_AddReference",
	"HkReferenceObject_RemoveReference",
	"HkReferenceObject_IsValid",
	"HkReferenceObject_DebugRemoveRef",
	"HkReferenceObject_ReferenceCount",
	"HkRigidBody_Create",
	"HkRigidBody_CreateWithCustomVelocity",
	"HkRigidBody_SetNumShapeKeysInContactPointProperties",
	"HkRigidBody_GetResponseModifiers",
	"HkRigidBody_SetResponseModifiers",
	"HkRigidBody_GetShape",
	"HkRigidBody_SetShape",
	"HkRigidBody_UpdateShape",
	"HkRigidBody_PredictRigidBodyMatrix",
	"HkRigidBody_SetMassProperties",
	"HkRigidBody_SetWorldMatrix",
	"HkRigidBody_SetTransform",
	"HkRigidBody_GetEnableDeactivation",
	"HkRigidBody_SetEnableDeactivation",
	"HkRigidBody_GetMarkedForVelocityRecompute",
	"HkRigidBody_SetMarkedForVelocityRecompute",
	"HkRigidBody_GetMotion",
	"HkRigidBody_GetMass",
	"HkRigidBody_SetMass",
	"HkRigidBody_GetCenterOfMassLocal",
	"HkRigidBody_SetCenterOfMassLocal",
	"HkRigidBody_GetInertiaTensor",
	"HkRigidBody_SetInertiaTensor",
	"HkRigidBody_GetInverseInertiaTensor",
	"HkRigidBody_SetInverseInertiaTensor",
	"HkRigidBody_GetCenterOfMassWorld",
	"HkRigidBody_GetCustomVelocity",
	"HkRigidBody_SetCustomVelocity",
	"HkRigidBody_GetDeltaAngle",
	"HkRigidBody_GetLinearDamping",
	"HkRigidBody_SetLinearDamping",
	"HkRigidBody_GetAngularDamping",
	"HkRigidBody_SetAngularDamping",
	"HkRigidBody_GetMaxLinearVelocity",
	"HkRigidBody_SetMaxLinearVelocity",
	"HkRigidBody_GetMaxAngularVelocity",
	"HkRigidBody_SetMaxAngularVelocity",
	"HkRigidBody_GetAllowedPenetrationDepth",
	"HkRigidBody_SetAllowedPenetrationDepth",
	"HkRigidBody_GetFriction",
	"HkRigidBody_SetFriction",
	"HkRigidBody_GetRestitution",
	"HkRigidBody_SetRestitution",
	"HkRigidBody_ApplyLinearImpulse",
	"HkRigidBody_ApplyPointImpulse",
	"HkRigidBody_ApplyAngularImpulse",
	"HkRigidBody_SetLayer",
	"HkRigidBody_GetCollisionFilterInfo",
	"HkRigidBody_SetCollisionFilterInfo",
	"HkRigidBody_ApplyForce",
	"HkRigidBody_ApplyForceToPoint",
	"HkRigidBody_ApplyTorque",
	"HkRigidBody_GetNativeObjectName",
	"HkRigidBody_RemoveFromWorld",
	"HkRigidBody_HasGravity",
	"HkRigidBody_HasConstraints",
	"HkRigidBody_GetBreakableBody",
	"HkRigidBody_GetGravity",
	"HkRigidBody_ReleaseGravity",
	"HkRigidBody_SetGravity",
	"HkRigidBody_Clone",
	"HkRigidBody_FromShape",
	"HkRigidBody_GetGcRoot",
	"HkRigidBody_GetGravityAction",
	"HkRigidBody_AddGravityAction",
	"HkRigidBody_GetDeactivationCounter0",
	"HkRigidBody_GetDeactivationCounter1",
	"HkRigidBody_HasActions",
	"HkRigidBodyCinfo_Create",
	"HkRigidBodyCinfo_Release",
	"HkRigidBodyCinfo_GetCollisionResponse",
	"HkRigidBodyCinfo_SetCollisionResponse",
	"HkRigidBodyCinfo_GetResponseModifiers",
	"HkRigidBodyCinfo_SetResponseModifiers",
	"HkRigidBodyCinfo_GetPosition",
	"HkRigidBodyCinfo_SetPosition",
	"HkRigidBodyCinfo_GetRotation",
	"HkRigidBodyCinfo_SetRotation",
	"HkRigidBodyCinfo_GetLinearVelocity",
	"HkRigidBodyCinfo_SetLinearVelocity",
	"HkRigidBodyCinfo_GetAngularVelocity",
	"HkRigidBodyCinfo_SetAngularVelocity",
	"HkRigidBodyCinfo_GetCenterOfMass",
	"HkRigidBodyCinfo_SetCenterOfMass",
	"HkRigidBodyCinfo_GetMass",
	"HkRigidBodyCinfo_SetMass",
	"HkRigidBodyCinfo_GetLinearDamping",
	"HkRigidBodyCinfo_SetLinearDamping",
	"HkRigidBodyCinfo_GetAngularDamping",
	"HkRigidBodyCinfo_SetAngularDamping",
	"HkRigidBodyCinfo_GetFriction",
	"HkRigidBodyCinfo_SetFriction",
	"HkRigidBodyCinfo_GetRestitution",
	"HkRigidBodyCinfo_SetRestitution",
	"HkRigidBodyCinfo_GetMaxLinearVelocity",
	"HkRigidBodyCinfo_SetMaxLinearVelocity",
	"HkRigidBodyCinfo_GetMaxAngularVelocity",
	"HkRigidBodyCinfo_SetMaxAngularVelocity",
	"HkRigidBodyCinfo_GetContactPointCallbackDelay",
	"HkRigidBodyCinfo_SetContactPointCallbackDelay",
	"HkRigidBodyCinfo_GetAllowedPenetrationDepth",
	"HkRigidBodyCinfo_SetAllowedPenetrationDepth",
	"HkRigidBodyCinfo_GetMotionType",
	"HkRigidBodyCinfo_SetMotionType",
	"HkRigidBodyCinfo_GetSolverDeactivation",
	"HkRigidBodyCinfo_SetSolverDeactivation",
	"HkRigidBodyCinfo_GetQualityType",
	"HkRigidBodyCinfo_SetQualityType",
	"HkRigidBodyCinfo_GetAutoRemoveLevel",
	"HkRigidBodyCinfo_SetAutoRemoveLevel",
	"HkRigidBodyCinfo_GetShape",
	"HkRigidBodyCinfo_SetShape",
	"HkRigidBodyCinfo_CalculateBoxInertiaTensor",
	"HkRigidBodyCinfo_CalculateSphereInertiaTensor",
	"HkRigidBodyCinfo_SetMassProperties",
	"HkRigidBodyCinfo_ComputeShapeMass",
	"HkSimulationIsland_GetEntityCount",
	"HkSimulationIsland_GetEntity",
	"HkSimulationIsland_GetBounds",
	"HkSimulationIsland_GetOffsets",
	"HkTaskProfiler_Init",
	"HkTaskProfiler_ReleaseResources",
	"HkTaskProfiler_HookJobQueue",
	"HkTaskProfiler_ReplayTimers",
	"HkTaskProfiler_Begin1",
	"HkTaskProfiler_Begin2",
	"HkTaskProfiler_Begin3",
	"HkTaskProfiler_Begin4",
	"HkTaskProfiler_Begin5",
	"HkTaskProfiler_End",
	"HkVDB_SyncTimers",
	"HkVDB_StepVDB",
	"HkVDB_Start",
	"HkVDB_ReleaseResources",
	"HkVDB_GetPort",
	"HkVDB_SetPort",
	"HkVDB_UpdateCamera",
	"HkVDB_Capture",
	"HkVDB_EndCapture",
	"HkWorld_Create",
	"HkWorld_CreateCInfo",
	"HkWorld_CreateBodyPairCollection",
	"HkWorld_RegisterWithJobQueue",
	"HkWorld_Lock",
	"HkWorld_Unlock",
	"HkWorld_LockCriticalOperations",
	"HkWorld_UnlockCriticalOperations",
	"HkWorld_ExecutePendingCriticalOperations",
	"HkWorld_StepDeltaTime",
	"HkWorld_StepMultiThreaded",
	"HkWorld_InitMtStep",
	"HkWorld_FinishMtStep",
	"HkWorld_ExecuteViolatedConstraintProjections",
	"HkWorld_ReportRuntimeDataConstraints",
	"HkWorld_AddConstraint",
	"HkWorld_RemoveConstraint",
	"HkWorld_AddEntity",
	"HkWorld_RemoveEntity",
	"HkWorld_AddPhantom",
	"HkWorld_RemovePhantom",
	"HkWorld_AddPhysicsSystem",
	"HkWorld_RemovePhysicsSystem",
	"HkWorld_GetPenetrationsShape",
	"HkWorld_GetPenetrationsBox",
	"HkWorld_GetPenetrationsShapeShape",
	"HkWorld_IsPenetratingShapeShape",
	"HkWorld_IsPenetratingShapeShapeTransform",
	"HkWorld_CastShape",
	"HkWorld_CastShapeReturnPoint",
	"HkWorld_CastShapeReturnContact",
	"HkWorld_CastShapeReturnContactData",
	"HkWorld_CastShapeReturnContactBodyData",
	"HkWorld_CastShapeReturnContactBodyDatas",
	"HkWorld_CastRayAll",
	"HkWorld_CastRayCollisionFilter",
	"HkWorld_CastRayFilterLayer",
	"HkWorld_MarkForWrite",
	"HkWorld_UnmarkForWrite",
	"HkWorld_RefreshCollisionFilterOnEntity",
	"HkWorld_RefreshCollisionFilterOnWorld",
	"HkWorld_ReintegrateEntity",
	"HkWorld_AddAction",
	"HkWorld_RemoveAction",
	"HkWorld_EnsureBatchSizes",
	"HkWorld_SetBatchBody",
	"HkWorld_AddEntityBatch",
	"HkWorld_RemoveEntityBatch",
	"HkWorld_GetActiveSimulationIslandsCount",
	"HkWorld_GetActiveSimulationIslandEntities",
	"HkWorld_DeactivateSimulationIslandRigidBodies",
	"HkWorld_IsActiveSimulationIsland",
	"HkWorld_GetConstraintCount",
	"HkWorld_GetActionCount",
	"HkWorld_GetFixedBody",
	"HkWorld_ReadSimulationIslandInfos",
	"HkWorld_GetGravity",
	"HkWorld_SetGravity",
	"HkWorld_GetDeactivationRotationSqrdA",
	"HkWorld_SetDeactivationRotationSqrdA",
	"HkWorld_GetDeactivationRotationSqrdB",
	"HkWorld_SetDeactivationRotationSqrdB",
	"HkWorld_AddWorldExtension",
	"HkWorld_Release",
	"HkPhysicsContext_Create",
	"HkPhysicsContext_RegisterAllPhysicsProcesses",
	"HkPhysicsContext_AddWorld",
	"HkPhysicsContext_RemoveWorld",
	"HkPhysicsContext_GetNumWorlds",
	"HkPhysicsContext_SyncTimers",
	"HkPhysicsContext_Release",
	"HkGroupFilter_Create",
	"HkGroupFilter_IsCollisionEnabled",
	"HkpAabbPhantom_Create",
	"HkpAabbPhantom_GetAabb",
	"HkpAabbPhantom_SetAabb",
	"HkpAabbPhantom_Release",
	"HkpCollidableAddedEvent_GetRigidBody",
	"HkpCollidableRemovedEvent_GetRigidBody",
	"HkSimpleShapePhantom_SetTransform",
	"HkSimpleShapePhantom_Create",
	"HkSimpleShapePhantom_CreateWithLayer",
	"HkSimpleShapePhantom_GetShape",
	"HkPhysicsSystem_IsActive",
	"HkPhysicsSystem_SetActive",
	"HkPhysicsSystem_RecreateConstraints",
	"HkPhysicsSystem_GetConstraintDataFromSystem",
	"HkPhysicsSystem_GetName",
	"HkPhysicsSystem_LoadRagdollFromFile",
	"HkPhysicsSystem_LoadRagdollFromBuffer",
	"HkPhysicsSystem_InitFromData",
	"HkpGroupFilter_CalcFilterInfo",
	"HkpGroupFilter_CalcFilterInfoFromCurrent",
	"HkpInertiaTensorComputer_OptimizeInertiasOfConstraintTree",
	"HkPhysicsSystem_Release",
	"HkRagdollConstraintData_GetPlaneMinAngularLimit",
	"HkRagdollConstraintData_SetPlaneMinAngularLimit",
	"HkRagdollConstraintData_GetPlaneMaxAngularLimit",
	"HkRagdollConstraintData_SetPlaneMaxAngularLimit",
	"HkRagdollConstraintData_GetTwistMinAngularLimit",
	"HkRagdollConstraintData_SetTwistMinAngularLimit",
	"HkRagdollConstraintData_GetTwistMaxAngularLimit",
	"HkRagdollConstraintData_SetTwistMaxAngularLimit",
	"HkRagdollConstraintData_GetMaxFrictionTorque",
	"HkRagdollConstraintData_SetMaxFrictionTorque",
	"HkRagdollConstraintData_SetInBodySpaceInternal",
	"HkRagdollConstraintData_SetAsymmetricConeAngle",
	"HkRagdollConstraintData_SetConeLimitStabilization",
	"HkBoxShape_Create",
	"HkBoxShape_CreateWithConvexRadius",
	"HkBoxShape_GetShapeFromCompoundShape",
	"HkBoxShape_GetHalfExtents",
	"HkBoxShape_SetHalfExtents",
	"HkBvCompressedMeshShape_CreateWithSimpleMesh",
	"HkBvCompressedMeshShape_CreateWithParams",
	"HkBvCompressedMeshShape_CreateUnsafe",
	"HkBvCompressedMeshShape_GetGeometry",
	"HkBvCompressedMeshShape_GetUserData",
	"HkBvShape_Create",
	"HkBvShape_GetChildShape",
	"HkBvShape_GetBoundingVolumeShape",
	"HkCapsuleShape_Create",
	"HkCapsuleShape_GetRadius",
	"HkCapsuleShape_GetVertexB",
	"HkCapsuleShape_GetVertexA",
	"HkCapsuleShape_GetCentre",
	"HkConvexShape_GetConvexShapeFromCompoundShape",
	"HkConvexShape_GetConvexRadius",
	"HkConvexShape_SetConvexRadius",
	"HkConvexShape_GetDefaultConvexRadius",
	"HkConvexTransformShape_Create",
	"HkConvexTransformShape_CreateTranslated",
	"HkConvexTransformShape_GetChildShape",
	"HkConvexTransformShape_GetTransform",
	"HkConvexTranslateShape_CreateWithChild",
	"HkConvexTranslateShape_GetChildShape",
	"HkConvexTranslateShape_GetTranslation",
	"HkConvexVerticesShape_Create",
	"HkConvexVerticesShape_CreateWithRadius",
	"HkConvexVerticesShape_GetCenter",
	"HkConvexVerticesShape_GetVertexCount",
	"HkConvexVerticesShape_GetFaceCount",
	"HkConvexVerticesShape_GetFaces",
	"HkConvexVerticesShape_GetVertices",
	"HkConvexVerticesShape_GetGeometry",
	"HkCylinderShape_Create",
	"HkCylinderShape_CreateWithConvexRadius",
	"HkCylinderShape_GetVertexB",
	"HkCylinderShape_GetVertexA",
	"HkCylinderShape_SetVertexB",
	"HkCylinderShape_SetVertexA",
	"HkCylinderShape_GetRadius",
	"HkCylinderShape_SetRadius",
	"HkCylinderShape_SetNumberOfVirtualSideSegments",
	"HkGridShape_Create",
	"HkGridShape_GetCellSize",
	"HkGridShape_GetShapeCount",
	"HkGridShape_SetDebugRigidBody",
	"HkGridShape_GetDebugRigidBody",
	"HkGridShape_SetDebugDraw",
	"HkGridShape_GetDebugDraw",
	"HkGridShape_AddShapes",
	"HkGridShape_Contains",
	"HkGridShape_GetShape",
	"HkGridShape_GetShapeInfo",
	"HkGridShape_GetShapeInfoCount",
	"HkGridShape_GetShapeMin",
	"HkGridShape_GetShapesInInterval",
	"HkGridShape_GetChildBounds",
	"HkGridShape_RemoveShapes",
	"HkGridShape_GetCellRanges",
	"HkListShape_Create",
	"HkListShape_GetDisabledChildrenCount",
	"HkListShape_GetTotalChildrenCount",
	"HkListShape_EnableShape",
	"HkListShape_GetChildByIndex",
	"HkListShape_IsChildEnabled",
	"HkMoppBvTreeShape_Create",
	"HkMoppBvTreeShape_GetShapeCollection",
	"HkMoppBvTreeShape_DisableKeys",
	"HkMoppBvTreeShape_QueryAABB",
	"HkMoppBvTreeShape_QueryPoint",
	"HkPhantomCallbackShape_Create",
	"HkShape_GetReferenceCount",
	"HkShape_GetShapeType",
	"HkShape_IsConvex",
	"HkShape_GetConvexRadius",
	"HkShape_SetConvexRadius",
	"HkShape_GetUserData",
	"HkShape_SetUserData",
	"HkShape_SetRigidBody",
	"HkShape_IsContainer",
	"HkShape_AddReference",
	"HkShape_RemoveReference",
	"HkShape_DisableRefCount",
	"HkShape_GetLocalAABB",
	"HkShape_CastRayCollectSingleHit",
	"HkShape_LoadShapeFromFile",
	"HkShape_GetContainer",
	"HkShapeBatch_GetCount",
	"HkShapeBatch_GetInfo",
	"HkShapeBatch_SetResult",
	"HkShapeBuffer_Create",
	"HkShapeBuffer_Destroy",
	"HkShapeCollection_GetShapeCount",
	"HkShapeCollection_GetShape",
	"HkShapeCollection_GetShapeWithBuffer",
	"HkShapeContainer_GetFirstKey",
	"HkShapeContainer_GetNextKey",
	"HkShapeContainer_CurrentValue",
	"HkShapeContainer_GetShape",
	"HkShapeContainer_IsShapeKeyValid",
	"HkShapeLoader_LoadShapesListFromBuffer",
	"HkShapeLoader_LoadShapesListFromFile",
	"HkShapeLoader_SaveShapesListToFile",
	"HkShapeLoader_CleanupShapesBuffer",
	"HkSimpleMeshShape_Create",
	"HkSmartListShape_Create",
	"HkSmartListShape_GetShapeCount",
	"HkSmartListShape_AddShape",
	"HkSmartListShape_RemoveShape",
	"HkSmartListShape_Validate",
	"HkSphereShape_Create",
	"HkSphereShape_GetRadius",
	"HkSphereShape_SetRadius",
	"HkStaticCompoundShape_Create",
	"HkStaticCompoundShape_GetInstanceCount",
	"HkStaticCompoundShape_AddInstance",
	"HkStaticCompoundShape_Bake",
	"HkStaticCompoundShape_ComposeShapeKey",
	"HkStaticCompoundShape_DecomposeShapeKey",
	"HkStaticCompoundShape_EnableAllShapeKeys",
	"HkStaticCompoundShape_EnableInstance",
	"HkStaticCompoundShape_EnableShapeKey",
	"HkStaticCompoundShape_GetFirstKey",
	"HkStaticCompoundShape_GetInstance",
	"HkStaticCompoundShape_GetInstanceTransform",
	"HkStaticCompoundShape_IsInstanceEnabled",
	"HkStaticCompoundShape_IsShapeKeyEnabled",
	"HkTransformShape_Create",
	"HkTransformShape_CreateWithTranslation",
	"HkTransformShape_GetTransform",
	"HkTransformShape_GetChildShape",
	"HkTriangleShape_GetExtrusion",
	"HkTriangleShape_GetPt2",
	"HkTriangleShape_GetPt1",
	"HkTriangleShape_GetPt0",
	"HkUniformGridShape_Create",
	"HkUniformGridShape_GetShapeCount",
	"HkUniformGridShape_DiscardLargeData",
	"HkUniformGridShape_GetHitsAndClear",
	"HkUniformGridShape_GetHitCellsInRange",
	"HkUniformGridShape_GetMissingCellsInRange",
	"HkUniformGridShape_InvalidateRange",
	"HkUniformGridShape_InvalidateRangeImmediate",
	"HkUniformGridShape_RemoveChild",
	"HkUniformGridShape_SetChild",
	"HkUniformGridShape_GetChild",
	"HkUniformGridShape_SetDeleteHandler",
	"HkUniformGridShape_RemoveShapeRequestHandler",
	"HkUniformGridShape_SetShapeRequestHandler",
	"HkUniformGridShape_EnableExtendedCache",
	"HkConstraintStabilizationUtil_StabilizeRagdollInertias",
	"HkDestructionUtils_FindAllBreakableShapesIntersectingSphere",
	"HkKeyFrameUtility_ApplyHardKeyFrame",
	"HkMassChangerUtil_Create",
	"HkMassChangerUtil_IsValid",
	"HkMassChangerUtil_Remove",
	"HkUtils_CalculateSeparatingVelocity",
	"HkUtils_SetSoftContact",
	"HkIntermediateBuffer_ReleaseUnmanaged",
	0
};

