// using UnityEngine;

// public class ChangeMaterialColor : MonoBehaviour
// {
//     [SerializeField] private Gradient colorGradient;
//     [SerializeField] private float value;
//     [SerializeField] private Target target;

//     private void OnEnable()
//     {
//         if (colorGradient == null)
//         {
//             colorGradient = new Gradient();
//             colorGradient.SetKeys(
//                 new GradientColorKey[] {
//                     new GradientColorKey(Color.red, 0.0f),
//                     new GradientColorKey(Color.yellow, 0.5f),
//                     new GradientColorKey(Color.green, 1.0f)
//                 },
//                 new GradientAlphaKey[] {
//                     new GradientAlphaKey(1.0f, 0.0f),
//                     new GradientAlphaKey(1.0f, 1.0f)
//                 }
//             );
//         }
//     }

//     void Start()
//     {

//     }

//     void Update()
//     {
//         value = target.currentHealth;
//         SetCellColorByValue(value);
//     }


//     public void SetCellColorByValue(float value)
//     {
//         Renderer renderer = GetComponent<Renderer>();
//         if (renderer == null)
//             return;

//         float t = Mathf.Clamp01(value / 100f);

//         Color newColor = colorGradient.Evaluate(t);

//         renderer.material.SetColor("_Cell_Color", newColor);
//     }
// }

