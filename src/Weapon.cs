using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class Weapon : MonoBehaviour{

    public Slider nockTimeSlider;

    public float offset;

    public GameObject projectile;
    public GameObject crossHair;
    public Transform shotPoint;

    private float timeBtwShots;
    public float startTimeBtwShots;

    //private float timeBtwTripleShots;
    //public float startTimeBtwTripleShots = 5f;
    private float nockTime;
    private float maxNockTime = 2f;

    public GameObject waterBomb;
    public GameObject oilBomb;

    private PlayerController playerController;
    //Настройка стрелы
    int type = 1;
    PlayerReturningProjectile.Elemental elemental = PlayerReturningProjectile.Elemental.Fire;

    private void Start(){
        nockTimeSlider.maxValue = maxNockTime;

    }

    private void Update(){
        playerController = GetComponentInParent<PlayerController>();
        Invoke("DelayForUpdate",0.00001f); //Для того, чтобы первый выстрел при использовании джойстика успел считать измененные координаты прицела
    }

    private void DelayForUpdate() {

        //Vector3 difference;
        //Для компа
        //Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        //Для джойстика
        //transform.localPosition = crossHair.transform.localPosition;
        Vector3 difference = new Vector3(crossHair.transform.localPosition.x, crossHair.transform.localPosition.y, 0.0f) - transform.localPosition;

        float rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotZ - offset - 90); //-90 надо бы убрать, но че со стрелами - хз

        if (timeBtwShots <= 0){
            //Для компа
            //if (Input.GetButtonDown("Fire1")){
            //    Instantiate(projectile, shotPoint.position, transform.rotation);
            //    timeBtwShots = startTimeBtwShots;
            //}
            //Для джойстика
            //if (Vector3.zero != new Vector3(CrossPlatformInputManager.GetAxis("AimHorizontal"), CrossPlatformInputManager.GetAxis("AimVertical"), 0.0f))
            //{
            //    Instantiate(projectile, shotPoint.position, transform.rotation); //transform.rotation
            //    timeBtwShots = startTimeBtwShots;
            //}

            if (Vector3.zero != new Vector3(CrossPlatformInputManager.GetAxis("AimHorizontal"), CrossPlatformInputManager.GetAxis("AimVertical"), 0.0f) && nockTime < maxNockTime){
                nockTime += Time.deltaTime;
            }
            else if ((Vector3.zero == new Vector3(CrossPlatformInputManager.GetAxis("AimHorizontal"), CrossPlatformInputManager.GetAxis("AimVertical"), 0.0f) || nockTime > maxNockTime) && nockTime > 0){
                GameObject arrow = Instantiate(projectile, shotPoint.position, transform.rotation); //transform.rotation
                arrow.GetComponent<PlayerReturningProjectile>().elemental = elemental;
                timeBtwShots = startTimeBtwShots;
                if (nockTime < maxNockTime){
                    arrow.GetComponent<PlayerReturningProjectile>().nockTime = nockTime;
                    //arrow.GetComponent<PlayerReturningProjectile>().speed = arrow.GetComponent<PlayerReturningProjectile>().speed * nockTime;
                }
                else{
                    arrow.GetComponent<PlayerReturningProjectile>().nockTime = maxNockTime;
                    //arrow.GetComponent<PlayerReturningProjectile>().speed = arrow.GetComponent<PlayerReturningProjectile>().speed * maxNockTime;
                }
                //print("Damage Arrow: " + arrow.GetComponent<PlayerReturningProjectile>().Damage + "  |  Speed Arrow: " + arrow.GetComponent<PlayerReturningProjectile>().speed + "  |  Nock Time: " + nockTime);
                nockTime = 0;
            }
            nockTimeSlider.value = nockTime;
            
        }
        //nockTimeSlider.GetComponentInChildren<Text>().text = ((stamina / 100).ToString() + "/" + (maxStamina / 100).ToString());
        else { timeBtwShots -= Time.deltaTime; }
        //print(playerController.waterBomb);
        if (CrossPlatformInputManager.GetButtonDown("WaterBomb") && playerController.waterBomb > 0){
            //Debug.Log("Water Bomb!");
            //GameObject wBomb = 
            Instantiate(waterBomb, transform.position, transform.rotation);
            playerController.waterBomb--;
            playerController.textWaterBomb.text = playerController.waterBomb.ToString();
        }
        if (CrossPlatformInputManager.GetButtonDown("OilBomb") && playerController.oilBomb > 0){
            //Debug.Log("Oil Bomb!");
            //GameObject oBomb = 
            Instantiate(oilBomb, transform.position, transform.rotation);
            playerController.oilBomb--;
            playerController.textOilBomb.text = playerController.oilBomb.ToString();
        }

        if (CrossPlatformInputManager.GetButtonDown("SwitchArrow")){
            Debug.Log("Switch Arrow");
            type++;
            if (type == 1) {
                elemental = PlayerReturningProjectile.Elemental.Fire;
                playerController.spriteSwitchArrow.color = new Color(255, 0, 0);
            }
            else if (type == 2) {
                elemental = PlayerReturningProjectile.Elemental.Cold;
                playerController.spriteSwitchArrow.color = new Color(0, 0, 255);
                //playerController.spriteSwitchArrow.color = new Color(0, 0, 255);
            }
            else if (type == 3) {
                elemental = PlayerReturningProjectile.Elemental.Lightning;
                playerController.spriteSwitchArrow.color = new Color(0, 255, 0);
                //playerController.spriteSwitchArrow.color = new Color(0, 255, 0);
                type = 0;
            }
            //playerController.textOilBomb = playerController.oilBomb.ToString();
        }
        //if (timeBtwTripleShots <= 0)
        //{
        //    if (CrossPlatformInputManager.GetButtonDown("TripleShot"))
        //    {
        //        Instantiate(projectile, shotPoint.position, transform.rotation);

        //        difference = new Vector3(crossHair.transform.localPosition.x + 0.3f, crossHair.transform.localPosition.y + 0.3f, 0.0f) - transform.localPosition;
        //        rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        //        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
        //        Instantiate(projectile, shotPoint.position, transform.rotation);

        //        difference = new Vector3(crossHair.transform.localPosition.x - 0.3f, crossHair.transform.localPosition.y - 0.3f, 0.0f) - transform.localPosition;
        //        rotZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        //        transform.rotation = Quaternion.Euler(0f, 0f, rotZ + offset);
        //        Instantiate(projectile, shotPoint.position, transform.rotation);
        //        timeBtwTripleShots = startTimeBtwTripleShots;
        //    }
        //}
        //else { timeBtwTripleShots -= Time.deltaTime; }
    }
}
