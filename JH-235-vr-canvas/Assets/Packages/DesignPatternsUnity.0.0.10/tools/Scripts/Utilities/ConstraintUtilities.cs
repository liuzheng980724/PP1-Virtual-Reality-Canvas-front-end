using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Snobal.DesignPatternsUnity_0_0.Utilities
{
    public static class ConstraintUtilities
    {
        public static ParentConstraint ApplyParentConstraint(Transform _parent, Transform _child, Vector3 localOffsetPos, Vector3 localOffsetRot, float _weight = 1f)
        {
            ParentConstraint constraint = UpdateConstraint<ParentConstraint>(_parent, _child, _weight);

            //constraint.locked = false;
            constraint.SetTranslationOffset(0, localOffsetPos);
            constraint.SetRotationOffset(0, localOffsetRot);
            //constraint.locked = true;
            constraint.constraintActive = true;

            return constraint;
        }

        public static ParentConstraint ApplyParentConstraint(Transform _parent, Transform _child, float _weight = 1f)
        {
            ParentConstraint constraint = UpdateConstraint<ParentConstraint>(_parent, _child, _weight);


            Vector3 positionOffset = _parent.InverseTransformPoint(_child.position);
            Quaternion rotationOffset = Quaternion.Inverse(_parent.rotation) * _child.transform.rotation;

            //constraint.locked = false;
            constraint.SetTranslationOffset(0, positionOffset);
            constraint.SetRotationOffset(0, rotationOffset.eulerAngles);
            //constraint.locked = true;
            constraint.constraintActive = true;

            return constraint;
        }

        private static T UpdateConstraint<T>(Transform _parent, Transform _child, float _weight)
            where T : Component, IConstraint
        {
            var source = new ConstraintSource() { sourceTransform = _parent, weight = _weight };

            var constraint = _child.gameObject.GetComponent<T>();

            if (constraint != null)
            {
                constraint.RemoveSource(0);
                constraint.AddSource(source);
                return constraint;
            }
            var newConstraint = _child.gameObject.AddComponent<T>();
            newConstraint.AddSource(source);
            return newConstraint;
        }
    }
}