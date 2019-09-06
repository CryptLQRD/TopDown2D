using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementsAndEffects : MonoBehaviour{

    //PlayerController owner1;
    public PlayerController player;
    public Enemy enemy;

    public int lightningResistance = 10;

    public bool burning = false; //Горю
    public bool chilling = false; //Под действием охлаждения
    public bool shocked = false; //Под воздействием шока

    private float timeIamInShock; //Оставшееся время воздействия шока

    //private float[] timeIamInColdProjectile = new float[] { 0, 0, 0 };    //Время горения от огненного снаряда 1,2,3
    //private float[] decreaseMovementSpeedPerColdProjectile = new float[] { 0, 0, 0 }; //Урон, получаемый за 1 тик 

    public bool iamInOil { set; get; } = false; //Стою в масле
    public bool iamInWater { set; get; } = false; //Стою в воде
    public bool iamInFire { set; get; } = false; //Стою в огне
    public bool iamSlowdown = false; //Нахожусь на замедляющей поверхности

    //private float startTimeIamInOil = 3f;
    private float decreaseSpeedIamSlowdown;
    public float durationTimeIamInOil; //общая длительность от бомб и масла на земле
    [HideInInspector]
    public float timeIamInOilBomb; // Сколько секунд на герое еще будет висеть Масло от бомбы (Снижается в приоритете перед timeIamInOil) 
    private float timeIamInOil;   //Сколько секунд на герое еще будет висеть Масло
    private float decreaseSpeedIamInOil;   //Понижение скорости передвижения героя при нахождении в масле
    private float decreaseSpeedIamInWater; //Понижение скорости передвижения героя при нахождении в воде + воздействии ледяных снарядов
    //private float startTimeIamInWater = 3f;
    public float durationTimeIamInWater; //общая длительность от бомб и воды на земле
    [HideInInspector]
    public float timeIamInWaterBomb; //Сколько секунд герой еще будет промокшим от бомбы (Снижается в приоритете перед timeIamInWater)
    private float timeIamInWater; //Сколько секунд герой еще будет промокшим
    //private float startTimeIamInFire = 3f;
    public float durationTimeIamInFire;  //Сколько секунд герой еще будет гореть: Снаряд + Огненная земля
    private float timeIamInFireGround;    //Время горения от огненной земли (Бочка с маслом подожженая) //Везде делать одинаковой
    //Попытка 1 с 3 элементами в массиве
    //private float[] timeIamInFireProjectile = new float[] { 0, 0, 0 };    //Время горения от огненного снаряда 1,2,3 (Если меняю кол-во элемов, то смотри ф-ю CheckDurationFireProjectile(int i) и CalculatedFireDuration() и OnFire(Collider2D col))
    //private float[] damagePerTickIamInFireProjectile = new float[] { 0, 0, 0 }; //Переделал в float //Урон, получаемый за 1 тик (Не учитываю разный урон от огня снарядов, т.к. сравниваю только время действия поджога от снарядов)
    //Попытка 2 с всего 2-мя переменными, но ошибкой после 2-х снарядов
    //private float timeIamInFireProjectile;    //Время горения от огненного снаряда 
    //private float damagePerTickIamInFireProjectile; //Переделал в float //Урон, получаемый за 1 тик
    

    [System.Serializable]
    public class FireProjectile{
        public FireProjectile(float timeElementalEffect, float elementalDamage) {
               timeIamInFireProjectile = timeElementalEffect;
               damagePerTickIamInFireProjectile = elementalDamage;
        }
        public float timeIamInFireProjectile;
        public float damagePerTickIamInFireProjectile;
    }
    public List<FireProjectile> fireProjectile = new List<FireProjectile>();

    [System.Serializable]
    public class ColdProjectile{
        public ColdProjectile(float timeElementalEffect, float decreaseSpeed) {
            timeIamInColdProjectile = timeElementalEffect;
            decreaseMovementSpeedPerColdProjectile = decreaseSpeed;
        }
        public float timeIamInColdProjectile;
        public float decreaseMovementSpeedPerColdProjectile;
    }
    public List<ColdProjectile> coldProjectile = new List<ColdProjectile>();

    private float startTickTimeIamInFire = 0.745f; //Периодический урон наносится каждые Х секунд. По умолчанию для врага. В старт меняется для героя
    private float tickTimeIamInFire; //Через сколько секунд нанесется урон.
    private int damagePerTickIamInFireGround; //Урон, получаемый за 1 тик
                                              //private int damagePerTickIamInFireTotal; //Суммарный урон, получаемый за 1 тик от damagePerTickIamInFireGround и damagePerTickIamInFireProjectile

    private void Start(){
        //if (player != null)
        //    startTickTimeIamInFire = 0.745f;
    }

    void Update(){
        //System.Type owner = typeof(PlayerController); //PlayerController.GetType();
        EffectCheck();
    }


    private void EffectCheck(){
        //print(iamInFire + "   |   " + timeIamInFireGround);
        if (chilling == true) {
            for (int i = 0; i < coldProjectile.Count; i++)
                CheckDurationColdProjectile(i);
            if (coldProjectile.Count == 0)
                chilling = false;
            CalculatedMovementSpeed();
        }
        /* С массивом рабочий вариант на 3 снаряда
        if (chilling == true) {
            for (int i = 0; i < timeIamInColdProjectile.Length; i++)
                CheckDurationColdProjectile(i);
            if (timeIamInColdProjectile[0] <= 0 && timeIamInColdProjectile[1] <= 0 && timeIamInColdProjectile[2] <= 0)
                chilling = false;
            CalculatedMovementSpeed();
        }
        */
        //шок
        if (shocked == true) {
            if (timeIamInShock > 0){
                if (enemy != null) enemy.movementSpeed = 0;
                timeIamInShock -= Time.deltaTime;
                if (timeIamInShock <= 0) {
                    timeIamInShock = 0;
                    shocked = false;
                    CalculatedMovementSpeed();
                }
            }
        }
        //Масло
        if (durationTimeIamInOil > 0) {
            if (timeIamInOilBomb > 0) {
                timeIamInOilBomb -= Time.deltaTime;
                if (timeIamInOilBomb < 0) {
                    if (timeIamInOil > 0 && iamInOil == false)
                        timeIamInOil += timeIamInOilBomb;
                    timeIamInOilBomb = 0;
                }
            }
            else if (timeIamInOil > 0 && iamInOil == false) {
                timeIamInOil -= Time.deltaTime;
                if (timeIamInOil < 0)
                    timeIamInOil = 0;
            }
            CalculatedOilDuration();
        }
        else if ((timeIamInOil > 0 || timeIamInOilBomb > 0) && durationTimeIamInOil <= 0)
            CalculatedOilDuration();
        //Вода
        if (durationTimeIamInWater > 0) {
            if (timeIamInWaterBomb > 0) {
                timeIamInWaterBomb -= Time.deltaTime;
                if (timeIamInWaterBomb < 0) {
                    if (timeIamInWater > 0 && iamInWater == false)
                        timeIamInWater += timeIamInWaterBomb;
                    timeIamInWaterBomb = 0;
                }
            }
            else if (timeIamInWater > 0 && iamInWater == false){
                timeIamInWater -= Time.deltaTime;
                if (timeIamInWater < 0)
                    timeIamInWater = 0;
            }
            CalculatedWaterDuration();
        }
        else if ((timeIamInWater > 0 || timeIamInWaterBomb > 0) && durationTimeIamInWater <= 0)
            CalculatedWaterDuration();
        //Огонь
        if (durationTimeIamInFire > 0 && burning == true) {
            //Требовалось для старого рассчета урона от огня и длительности снарядов
            //if (timeIamInFireProjectile[0] > 0 || timeIamInFireProjectile[1] > 0 || timeIamInFireProjectile[2] > 0) {
            //    for (int i = 0; i < timeIamInFireProjectile.Length; i++)
            //        CheckDurationFireProjectile(i);
            //}
            //2 попытка
            //if (timeIamInFireProjectile > 0) {
            //    CheckDurationFireProjectile();
            //}
            //3 попытка
            if (fireProjectile.Count > 0) {
                for (int i = 0; i < fireProjectile.Count; i++)
                    CheckDurationFireProjectile(i);
            }
            else if (timeIamInFireGround > 0 && iamInFire == false) {
                timeIamInFireGround -= Time.deltaTime;
                if (timeIamInFireGround <= 0) {
                    timeIamInFireGround = 0;
                    damagePerTickIamInFireGround = 0;
                }
            }
            CalculatedFireDuration();
            //print("DurationTimeIamInFire: " + durationTimeIamInFire);
            if (durationTimeIamInFire <= 0) {
                burning = false;
                tickTimeIamInFire = 0;
            }
            else {
                if (tickTimeIamInFire <= 0) { //Периодический урон
                    //print(durationTimeIamInFire + "   УРОН!");
                    tickTimeIamInFire = startTickTimeIamInFire;
                    //При изменении кол-ва снарядов огня действующих одновременно изменить и расчет health
                    //1 попытка Требовалось для старого рассчета урона от огня и длительности снарядов
                    //if (enemy != null) enemy.Health -= (damagePerTickIamInFireGround + damagePerTickIamInFireProjectile[0] + damagePerTickIamInFireProjectile[1] + damagePerTickIamInFireProjectile[2]);
                    //else if (player != null) player.Health -= (damagePerTickIamInFireGround + damagePerTickIamInFireProjectile[0] + damagePerTickIamInFireProjectile[1] + damagePerTickIamInFireProjectile[2]);
                    //2 попытка
                    //if (enemy != null) enemy.Health -= (damagePerTickIamInFireGround + damagePerTickIamInFireProjectile);
                    //else if (player != null) player.Health -= (damagePerTickIamInFireGround + damagePerTickIamInFireProjectile);
                    //3-я попытка
                    float totalDamagePerTickIamInFireProjectile = 0;
                    for (int i = 0; i < fireProjectile.Count; i++)
                        totalDamagePerTickIamInFireProjectile += fireProjectile[i].damagePerTickIamInFireProjectile;
                    if (enemy != null) enemy.Health -= (damagePerTickIamInFireGround + totalDamagePerTickIamInFireProjectile);
                    else if (player != null) player.Health -= (damagePerTickIamInFireGround + totalDamagePerTickIamInFireProjectile);
                }
                else tickTimeIamInFire -= Time.deltaTime;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.tag == "Water") {
            OnWater(0, 0);
            if (player != null) {
                player.waterBomb = 5;
                player.textWaterBomb.text = player.waterBomb.ToString();
            }
        }
    }
    private void OnCollisionStay2D(Collision2D col) {
        if (col.gameObject.tag == "Water")
            OnWater(0, 0);
    }
    private void OnCollisionExit2D(Collision2D col) {
        if (col.gameObject.tag == "Water"){
            iamInWater = false;
            decreaseSpeedIamInWater = 0;
        }
    }

    /* Попытка 1 С массивом рабочая
    private void CalculatedMovementSpeed(){//Настроено только для 3-х элементов (0.1.2) иначе нужен добавить еще элемент
        if (enemy != null) enemy.movementSpeed = enemy.startMovementSpeed - enemy.startMovementSpeed * (decreaseSpeedIamInOil + (decreaseMovementSpeedPerColdProjectile[0] + decreaseMovementSpeedPerColdProjectile[1] + decreaseMovementSpeedPerColdProjectile[2]) * (decreaseSpeedIamInWater + 1f));
        else if (player != null) player.movementSpeed = player.startMovementSpeed - player.startMovementSpeed * (decreaseSpeedIamInOil + (decreaseMovementSpeedPerColdProjectile[0] + decreaseMovementSpeedPerColdProjectile[1] + decreaseMovementSpeedPerColdProjectile[2]) * (decreaseSpeedIamInWater + 1f));
    }

    private void CheckDurationColdProjectile(int i) { //Настроено только для 3-х элементов (0.1.2) иначе нужен for или добавить еще элемент в if
        if (timeIamInColdProjectile[i] > 0) {
            timeIamInColdProjectile[i] -= Time.deltaTime;
            if (timeIamInColdProjectile[i] <= 0){
                timeIamInColdProjectile[i] = 0;
                decreaseMovementSpeedPerColdProjectile[i] = 0;
            }
        }
    }

    public void OnColdProjectile(float timeElementalEffect, float decreaseSpeed, float elementalDamage){
        chilling = true;
        if (enemy != null) enemy.Health -= elementalDamage;
        else if (player != null) player.Health -= elementalDamage;
        int tempI = -1;
        float tempTimeIamInColdProjectile = timeElementalEffect;
        for (int i = 0; i < timeIamInColdProjectile.Length; i++) //Ищем наименьшее оставшееся время
            if (tempTimeIamInColdProjectile > timeIamInColdProjectile[i])
                if (timeIamInColdProjectile[i] <= 0){ //Если <= 0, то присваиваем знаячение в эту переменную и выходим из цикла
                    timeIamInColdProjectile[i] = tempTimeIamInColdProjectile;
                    decreaseMovementSpeedPerColdProjectile[i] = decreaseSpeed;
                    break;
                }
                else{
                    tempTimeIamInColdProjectile = timeIamInColdProjectile[i];
                    tempI = i;
                }
        if (tempI != -1){
            timeIamInColdProjectile[tempI] = timeElementalEffect;
            decreaseMovementSpeedPerColdProjectile[tempI] = decreaseSpeed;
        }
        CalculatedMovementSpeed();
    }
    */

    private void CalculatedMovementSpeed(){
        float tempMovementSpeed = 0;
        if (enemy != null) tempMovementSpeed = enemy.startMovementSpeed;
        else if (player != null) tempMovementSpeed = player.startMovementSpeed;
        
        if (iamInOil == true)
            tempMovementSpeed -= tempMovementSpeed * decreaseSpeedIamInOil;
        if (iamSlowdown == true)
            tempMovementSpeed -= tempMovementSpeed * decreaseSpeedIamSlowdown;
        for (int i = 0; i < coldProjectile.Count; i++)
            tempMovementSpeed = tempMovementSpeed - tempMovementSpeed*coldProjectile[i].decreaseMovementSpeedPerColdProjectile * (decreaseSpeedIamInWater + 1f); //Так же умножаем на 120% силу замедляющего эффекта, если в воде иначе 100% т.е. не меняем

        if (enemy != null) enemy.movementSpeed = tempMovementSpeed; //enemy.startMovementSpeed - enemy.startMovementSpeed * (decreaseSpeedIamInOil + (decreaseMovementSpeedPerColdProjectile[0] + decreaseMovementSpeedPerColdProjectile[1] + decreaseMovementSpeedPerColdProjectile[2]) * (decreaseSpeedIamInWater + 1f));
        else if (player != null) player.movementSpeed = tempMovementSpeed; //player.movementSpeed = player.startMovementSpeed - player.startMovementSpeed * (decreaseSpeedIamInOil + (decreaseMovementSpeedPerColdProjectile[0] + decreaseMovementSpeedPerColdProjectile[1] + decreaseMovementSpeedPerColdProjectile[2]) * (decreaseSpeedIamInWater + 1f));
    }

    private void CheckDurationColdProjectile(int i) { //Настроено только для 3-х элементов (0.1.2) иначе нужен for или добавить еще элемент в if
        if (coldProjectile[i].timeIamInColdProjectile > 0) {
            coldProjectile[i].timeIamInColdProjectile -= Time.deltaTime;
            if (coldProjectile[i].timeIamInColdProjectile <= 0){
                coldProjectile.RemoveAt(i);
                i--;
            }
        }
    }

    public void OnColdProjectile(float timeElementalEffect, float decreaseSpeed, float elementalDamage){
        chilling = true;
        if (enemy != null) enemy.Health -= elementalDamage;
        else if (player != null) player.Health -= elementalDamage;
        coldProjectile.Add(new ColdProjectile(timeElementalEffect, decreaseSpeed));
        CalculatedMovementSpeed();
    }

    public void CalculatedFireDuration() {
        durationTimeIamInFire = timeIamInFireGround;
        for (int i = 0; i < fireProjectile.Count; i++)
            durationTimeIamInFire += fireProjectile[i].timeIamInFireProjectile;
    }

    private void CheckDurationFireProjectile(int i) { //Настроено только для 3-х элементов (0.1.2) иначе нужен for или добавить еще элемент в if
        if (fireProjectile[i].timeIamInFireProjectile > 0) {
            fireProjectile[i].timeIamInFireProjectile -= Time.deltaTime;
            if (fireProjectile[i].timeIamInFireProjectile <= 0) {
                if (fireProjectile.Count - 1 == i)
                    if (timeIamInFireGround > 0 && iamInFire == false)
                        timeIamInFireGround += fireProjectile[i].timeIamInFireProjectile;
                fireProjectile.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnFire(Collider2D col, bool burn, float startDurationFWO, int damagePerTickFire) {
        if (burn == true && timeIamInFireGround != startDurationFWO) { //startTimeIamInFire)
            timeIamInFireGround = startDurationFWO; // startTimeIamInFire;
            CalculatedFireDuration();
            damagePerTickIamInFireGround = damagePerTickFire;
            iamInFire = true;
            if (burning == false) {
                burning = true;
                tickTimeIamInFire = startTickTimeIamInFire;
            }
        }
    }

    public void OnFireProjectile(float timeElementalEffect, float elementalDamage) {
        if (burning == false) {
            burning = true;
            tickTimeIamInFire = startTickTimeIamInFire;
        }
        fireProjectile.Add(new FireProjectile (timeElementalEffect, elementalDamage)); //fireProjectile[i].timeIamInFireProjectile,
        //timeIamInFireProjectile[tempI] = timeElementalEffect;
        //damagePerTickIamInFireProjectile[tempI] = elementalDamage;
        CalculatedFireDuration();
    }

    public void OnLightningProjectile(float timeElementalEffect, float elementalDamage){
        int rand = Random.Range(-0, 101);
        Debug.Log("rand = " + rand);
        if (enemy != null) enemy.Health -= elementalDamage;
        else if (player != null) player.Health -= elementalDamage;
        if (lightningResistance < rand || (iamInWater == true && iamInOil == false)) {
            shocked = true;
            timeIamInShock = timeElementalEffect;
            if (enemy != null) enemy.Health -= elementalDamage/2;
            else if (player != null) player.Health -= elementalDamage/2;
        }
    }

    public void CalculatedOilDuration() {
        durationTimeIamInOil = timeIamInOil + timeIamInOilBomb;
    }
    public void CalculatedWaterDuration(){
        durationTimeIamInWater = timeIamInWater + timeIamInWaterBomb;
    }

    private void OnWater(float startDurationFWO, float decreaseSpeed, Collider2D col = null){
        iamInWater = true;
        if (iamInOil == false){
            if (col != null) {// startTimeIamInWater;
                timeIamInWater = startDurationFWO;//col.GetComponent<GroundEffect>().startDurationFWO;
                decreaseSpeedIamInWater = decreaseSpeed; //col.GetComponent<GroundEffect>().decreaseSpeed;
            }
            else {
                timeIamInWater = 4.5f;
                decreaseSpeedIamInWater = 0.2f;
                Debug.Log("Water in lake!");
            }
            iamInFire = false;
            burning = false;
            durationTimeIamInFire = 0;
            timeIamInFireGround = 0;
            //1 попытка Требовалось для старого рассчета времени огня и урона от снарядов
            //for (int i = 0; i < timeIamInFireProjectile.Length; i++){
            //    timeIamInFireProjectile[i] = 0;
            //    damagePerTickIamInFireProjectile[i] = 0;
            //}
            //2 попытка
            //timeIamInFireProjectile = 0;
            //damagePerTickIamInFireProjectile = 0;
            //3 попытка 
            for (int i = 0; i < fireProjectile.Count; i++){
                fireProjectile.RemoveAt(i);
                i--;
                //timeIamInFireProjectile[i] = 0;
                //damagePerTickIamInFireProjectile[i] = 0;
            }
            damagePerTickIamInFireGround = 0;
            if (durationTimeIamInOil > 0){
                timeIamInOil = 0;
                timeIamInOilBomb = 0;
                CalculatedOilDuration();
            }
        }
    }

    public void Slowdown(bool selector, float decreaseSpeed) {
        iamSlowdown = selector;
        decreaseSpeedIamSlowdown = decreaseSpeed;
        CalculatedMovementSpeed();
    }

    public void GETriggerEnter(Collider2D col, GroundEffect.Name name, float startDurationFWO, float decreaseSpeed, bool burn, int damagePerTickFire) {
        switch (name) { //GameObject gameObj
            case GroundEffect.Name.Oil:
                timeIamInOil = startDurationFWO;//startTimeIamInOil;
                iamInOil = true;
                if (durationTimeIamInWater > 0) {
                    timeIamInWater = 0;
                    timeIamInWaterBomb = 0;
                    CalculatedWaterDuration();
                }
                decreaseSpeedIamInOil = decreaseSpeed;
                decreaseSpeedIamInWater = 0f;
                CalculatedMovementSpeed();
                OnFire(col, burn, startDurationFWO, damagePerTickFire);
                break;
            case GroundEffect.Name.Water:
                OnWater(startDurationFWO, decreaseSpeed, col);
                break;
        }
    }

    public void GETriggerStay(Collider2D col, GroundEffect.Name name, float startDurationFWO, float decreaseSpeed, bool burn, int damagePerTickFire) {
        switch (name) {
            case GroundEffect.Name.Oil:
                iamInOil = true;
                if (decreaseSpeedIamInOil != decreaseSpeed){
                    decreaseSpeedIamInOil = decreaseSpeed;
                    CalculatedMovementSpeed();
                }
                if (timeIamInOil != startDurationFWO) { //startTimeIamInOil) {
                    timeIamInOil = startDurationFWO; // startTimeIamInOil;
                }
                OnFire(col, burn, startDurationFWO, damagePerTickFire);
                break;
            case GroundEffect.Name.Water:
                OnWater(startDurationFWO, decreaseSpeed, col);
                break;
        }
    }

    public void GETriggerExit(GroundEffect.Name name, bool burn) {
        switch (name) {
            case GroundEffect.Name.Oil:
                iamInOil = false;
                decreaseSpeedIamInOil = 0;
                CalculatedMovementSpeed();
                if (burn == true) {
                    iamInFire = false;
                }
                //col.GetComponent<GroundEffect>().FireOil();
                break;
            case GroundEffect.Name.Water:
                iamInWater = false;
                decreaseSpeedIamInWater = 0;
                break;
        }
    }

}
