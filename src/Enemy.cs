using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//namespace Pathfinding

public class Enemy : MonoBehaviour {

    ElementsAndEffects elementsAndEffects;

    private float lifeTime;
    private bool iAmSummon = false;
    private float maxHealth;
    public float MaxHealth {
        get { return maxHealth; }
        set { maxHealth = value; }
    }
    public float health = 20;
    public float Health {
        get { return health; }
        set { health = value; }
    }
    public float startMovementSpeed = 1f;
    public float movementSpeed;
    private float tBShots; //timeBtwShots
    public float sTBShots = 2f; //startTimeBtwShots //Attack Speed
    public int damage = 1;
    public GameObject projectile;
    public int elementalDamage = 0;
    public float timeElementalEffect = 0; //2.35f;
    public EnemyProjectile.Elemental elemental;
    //public enum movementType { Flying, Walking, Swimming }
    private int keyNum = -1;
    private bool lootOn = true;

    private GameObject castingSpell; //Способность, которую будет произносить
    private bool castSpell = false;
    public GameObject spellSummon;
    private int iSummon; //Если грубо, то кол-во призываемых существ
    public int maxSummon; //Если грубо, то кол-во призываемых существ
    private float tBCastsSpellSummon;
    public float sTBCastsSpellSummon = 50f;
    public GameObject spell_1;
    private float tBCastsSpell_1;
    public float sTBCastsSpell_1 = 15f;
    public GameObject spell_2;
    private float tBCastsSpell_2;
    public float sTBCastsSpell_2 = 25f;

    private Vector3 spellPos;

    private bool faceRight = true;
    private Rigidbody2D rb;

    //public string algorithm; //Алгоритм Врага
    private bool colEnvironment = false;
    public bool ColEnvironment {
        get { return colEnvironment; }
    }
    public Algorithm algorithm;
    public enum Algorithm { MeleeAStar, RangeEasy, RangeAStar, Boss1, None }; //EasyMelee

    public float distance; //Текущая дистанция от Цели до Врага
    public float minimalDistance = 1.9f; //Для низа 2.8f т.е. +0.9f //Минимальная дистанция, до которой может сократить расстояние Враг
    public float radius = 16; //Радиус обнаружения Цели

    private Animator _animatorController;
    //Для WalkingAndWander
    //private float walkingRange = 1f;
    //private float minX;
    //private float maxX;
    //private float minY;
    //private float maxY;
    //private float waitTime;
    //public float startWaitTime = 8f;
    //private Vector3 myStartPosition;
    [HideInInspector]
    public Vector3 myNewPosition;

    private bool iAmDead = false;

    private Transform target;

    private SpecialEffect specialEffect;
    private Pathfinding.AILerp AILerp;
    private Pathfinding.Seeker Seeker;
    private Pathfinding.EnemyAIDest EnemyAIDest;
    //private float retreatDistance = minimalDistance; //Дистанция отступления

    //public GameObject deathEffect;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        _animatorController = GetComponent<Animator>();
        
        myNewPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z); //myStartPosition (myStartPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z));
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        maxHealth = health;
        movementSpeed = startMovementSpeed;

        if (spellSummon != null || spell_1 != null || spell_2 != null) specialEffect = GetComponent<SpecialEffect>();
        AILerp = this.GetComponent<Pathfinding.AILerp>();
        Seeker = this.GetComponent<Pathfinding.Seeker>();
        EnemyAIDest = this.GetComponent<Pathfinding.EnemyAIDest>();

        elementsAndEffects = this.GetComponent<ElementsAndEffects>();
        //Для WalkingAndWander
        //waitTime = startWaitTime;
        //minX = transform.position.x - walkingRange;
        //maxX = transform.position.x + walkingRange;
        //minY = transform.position.y - walkingRange;
        //maxY = transform.position.y + walkingRange;
    }

    void Update(){
        if (iAmSummon == true && iAmDead == false){
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0)
                health = 0;
        }
        if (health <= 0 && iAmDead == false){
            //Instantiate(deathEffect, transform.position, Quaternion.identity);
            iAmDead = true;
            _animatorController.SetInteger("Switch", 4);
            algorithm = Algorithm.None;
            Trigger(false);
            if (lootOn == true)
                GetComponent<LootDrop>().calculateLoot(transform.position);
            if (keyNum != -1)
                GetComponent<LootDrop>().createKey(transform.position, keyNum);
            Destroy(gameObject, 1.25f);
        }
    }

    // Update is called once per frame
    void FixedUpdate(){
        //dist = Vector2.Distance(Player.transform.position, transform.position);
        distance = Vector2.Distance(transform.position, target.position);
        if (elementsAndEffects.shocked == false)
        switch (algorithm) { //В каждом case нужно ставить в конце break
            //case Algorithm.EasyMelee:
            //    EasyMeleeAlg();
            //    break;
            case Algorithm.RangeEasy:
                EasyRangeAlg();
                break;
            case Algorithm.MeleeAStar:
                AStarMeleeAlg();
                break;
            case Algorithm.Boss1:
                //if (timeBtwCasts > 0) timeBtwCasts -= Time.deltaTime;
                if (distance < radius){

                    //print("tBCastsSpellSummon: " + tBCastsSpellSummon + "  |  tBCastsSpell_1:" + tBCastsSpell_1 + "  |  tBCastsSpell_2:" + tBCastsSpell_2);
                    UseSummon();
                    //if (tBCastsSpellSummon <= 0 && castSpell == false){
                    //    tBCastsSpellSummon = sTBCastsSpellSummon;
                    //    castSpell = true;
                    //    _animatorController.SetInteger("Switch", 5);
                    //    for (iSummon = 0; iSummon < maxSummon; iSummon++){ //Создаем 2-х гоблинов
                    //        if (iSummon % 2 == 0) spellPos = new Vector3(transform.position.x - 0.2f, transform.position.y - 0.1f, transform.position.z);
                    //        if (iSummon % 2 == 1) spellPos = new Vector3(transform.position.x + 0.1f, transform.position.y - 0.1f, transform.position.z);
                    //        GetComponent<BossEffect>().CreatePurpleFlame(spellPos, 2.5f); //Анимация пламени
                    //        Invoke("CastSpellSummon", 2f); //Создание заклинания
                    //    }
                    //}
                    UseEarthSpike();
                    //if (tBCastsSpell_1 <= 0 && castSpell == false){
                    //    castingSpell = spell_1;
                    //    tBCastsSpell_1 = sTBCastsSpell_1;
                    //    castSpell = true;
                    //    _animatorController.SetInteger("Switch", 5);
                    //    //GetComponent<BossEffect>().CreateRedZone(spellPos, 1.5f);
                    //    spellPos = new Vector3(target.position.x, target.position.y - 0.25f, target.position.z); // Визуальный эффект под ногами героя
                    //    GetComponent<BossEffect>().CreateEarthSpikeGround(spellPos, 1.5f); //Создание визуального эффекта 
                    //    spellPos = new Vector3(target.position.x, target.position.y + 0.2f, target.position.z); // Появления шипа под ногами героя
                    //    Invoke("CastSpell", 1.2f); //Создание заклинания (spell1)
                    //}
                    //CAST2
                    UseEarthenBoulder();

                    if (castSpell == true) //Включаю обратно в завершающих заклинания функциях (CastSpell и CastSpellSummon)
                        Trigger(false);
                }

                if (tBCastsSpell_1 > 0) { tBCastsSpell_1 -= Time.deltaTime; }
                if (tBCastsSpell_2 > 0) { tBCastsSpell_2 -= Time.deltaTime; }
                if (tBCastsSpellSummon > 0) { tBCastsSpellSummon -= Time.deltaTime; }
                if (castSpell == false)
                    AStarMeleeAlg();
                break;
            default: break;
        }
    }

    void moveEnemy(float speedX, float speedY){ //Не использую
        rb.velocity = new Vector2(speedX, speedY);
        if (speedX < 0 && !faceRight)
            flip();
        else if (speedX > 0 && faceRight)
            flip();
    }

    void checkForFlip(){
        if (target.transform.position.x < transform.position.x && faceRight)
            flip();
        else if (target.transform.position.x > transform.position.x && !faceRight)
            flip();
    }
    void flip(){
        faceRight = !faceRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    //void EasyMeleeAlg(){
    //    if (tBShots > 0) { tBShots -= Time.deltaTime; }
    //    if (distance > radius){
    //        if (algorithm != Algorithm.Boss1)
    //            Walking();
    //        else WalkingBoss();
    //    }
    //    else if (distance < minimalDistance + 0 || (distance < minimalDistance + minimalDistance/2 && target.position.y > transform.position.y + 1)){
    //        checkForFlip();
    //        MeleeAttack(0);//moveEnemy(0, 0); //Attack
    //    }
    //    else if (distance < radius && distance < minimalDistance + 0.12f){ //Если почти подошел
    //        checkForFlip();
    //        transform.position = Vector2.MoveTowards(transform.position, target.position, movementSpeed * Time.deltaTime);
    //        MeleeAttack(1);
    //    }
    //    else if (distance < radius){
    //        checkForFlip();
    //        transform.position = Vector2.MoveTowards(transform.position, target.position, movementSpeed * Time.deltaTime);
    //        _animatorController.SetInteger("Switch", 1);
    //    }
    //}

    void AStarMeleeAlg(){
        if (tBShots > 0) { tBShots -= Time.deltaTime; }
        if (distance > radius){
            Trigger(false);
            Walking();
            if (distance >= radius * 1.5f)
                transform.position = myNewPosition;
        }
        else if (distance < minimalDistance || (distance < minimalDistance + minimalDistance/1.15f && target.position.y > transform.position.y + 1)){
            checkForFlip();
            MeleeAttack(0);//moveEnemy(0, 0); //Attack
            Trigger(false);
        }
        else if (distance < radius && distance < minimalDistance + 0.12f){ //Если почти подошел
            checkForFlip();
            transform.position = Vector2.MoveTowards(transform.position, target.position, movementSpeed * Time.deltaTime);
            MeleeAttack(1);
            Trigger(false);
        }
        else if (distance < radius && colEnvironment == false){
            checkForFlip();
            transform.position = Vector2.MoveTowards(transform.position, target.position, movementSpeed * Time.deltaTime);
            _animatorController.SetInteger("Switch", 1);
        }
        else if (distance < radius && colEnvironment == true){
            //Trigger(true);
            checkForFlip();
            _animatorController.SetInteger("Switch", 1);
        }
    }

    void EasyRangeAlg() {
        if (tBShots > 0) { tBShots -= Time.deltaTime; }
        if (distance > radius){
            Walking();
        }
        else if (distance < minimalDistance || (distance < minimalDistance && target.position.y > transform.position.y + 1)){
            checkForFlip();
            transform.position = Vector2.MoveTowards(transform.position, target.position, -movementSpeed * Time.deltaTime); //moveEnemy(0, 0);
            RangeAttack(1);
        }
        else if (distance < radius && distance > minimalDistance + 0.5f){
            checkForFlip();
            transform.position = Vector2.MoveTowards(transform.position, target.position, movementSpeed * Time.deltaTime);
            _animatorController.SetInteger("Switch", 1);
            //RangeAttack(1);
        }
        else if (distance >= minimalDistance && distance <= minimalDistance + 0.5f){ //Расстояние Между minimalDistance и retreatDistance (это minimalDistance + const)
            checkForFlip();
            RangeAttack(0);
        }
    }

    /*void WalkingAndWander(){ // Возвращается на место + блуждает
        float dist = Vector2.Distance(transform.position, myNewPosition); //Расчет дистанции между текущей позицией и новой позицией
        if (dist > 0.01f){
            transform.position = Vector2.MoveTowards(transform.position, myNewPosition, movementSpeed * Time.deltaTime);
            _animatorController.SetInteger("Switch", 1);
        }
        if (dist < 0.2f){ //Если достигнута точка, тогда ждем пока не выйдет время и ищем новую
            if (waitTime <= 0){
                myNewPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
                waitTime = startWaitTime;
            }
            else{
                waitTime -= Time.deltaTime;
                _animatorController.SetInteger("Switch", 0);
            }
        }
        else if (dist > 0.2f && (myNewPosition.x < transform.position.x && faceRight)) flip();
        else if (dist > 0.2f && (myNewPosition.x > transform.position.x && !faceRight)) flip();

        //Постоянное блуждание
        //transform.position = Vector2.MoveTowards(transform.position, moveSpot.position, movementSpeed * Time.deltaTime);
        //if(Vector2.Distance(transform.position, moveSpot.position) < 0.2f){
        //    moveSpot.position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        //}
    }
    */

    void Walking(){ //Возвращается на место
        float dist = Vector2.Distance(transform.position, myNewPosition); //Расчет дистанции между текущей позицией и новой позицией
        if (dist > 0.01f){
            transform.position = Vector2.MoveTowards(transform.position, myNewPosition, movementSpeed * Time.deltaTime);
            _animatorController.SetInteger("Switch", 1);
        }
        if (dist < 0.2f){ //Если достигнута точка, тогда ждем пока не выйдет время и ищем новую
            _animatorController.SetInteger("Switch", 0);
        }
        else if (dist > 0.2f && (myNewPosition.x < transform.position.x && faceRight)) flip(); 
        else if (dist > 0.2f && (myNewPosition.x > transform.position.x && !faceRight)) flip();
    }

    void MeleeAttack(int aniSwitch){ //Передаю анимацию, которую воспроивожу между выстрелами
        if (tBShots <= 0){
            _animatorController.SetInteger("Switch", 2);
            Invoke("DelayToDamageTarget", 0.3f);
            tBShots = sTBShots;
        }
        //else timeBtwShots -= Time.deltaTime;
    }
    void DelayToDamageTarget(){
        target.GetComponent<PlayerController>().ChangeHealth(-damage);
    }

    void RangeAttack(int aniSwitch){ //Передаю анимацию, которую воспроивожу между выстрелами
        if (tBShots <= 0){
            _animatorController.SetInteger("Switch", 2);
            Invoke("CreateProjectile", 0.4f);
            tBShots = sTBShots;
        }
        else{
            _animatorController.SetInteger("Switch", aniSwitch);
            //timeBtwShots -= Time.deltaTime;
        }
    }
    void CreateProjectile(){
        GameObject newProj = Instantiate(projectile, transform.position, Quaternion.identity);
        newProj.GetComponent<EnemyProjectile>().ProjectileOptions(damage, elementalDamage, elemental, timeElementalEffect, 0.15f);
    }

    void UseEarthSpike() {
        if (tBCastsSpell_1 <= 0 && castSpell == false){
            castingSpell = spell_1;
            tBCastsSpell_1 = sTBCastsSpell_1;
            castSpell = true;
            _animatorController.SetInteger("Switch", 5);
            spellPos = new Vector3(target.position.x, target.position.y - 0.25f, target.position.z); // Визуальный эффект под ногами героя
            specialEffect.CreateEarthSpikeGround(spellPos, 1.5f); //Создание визуального эффекта 
            spellPos = new Vector3(target.position.x, target.position.y + 0.2f, target.position.z); // Появления шипа под ногами героя
            Invoke("CastSpell", 1.2f); //Создание заклинания (spell1)
        }
    }

    void UseEarthenBoulder(){
        if (tBCastsSpell_2 <= 0 && castSpell == false){
            castingSpell = spell_2;
            tBCastsSpell_2 = sTBCastsSpell_2;
            castSpell = true;
            _animatorController.SetInteger("Switch", 5);
            spellPos = new Vector3(transform.position.x, transform.position.y, transform.position.z); // Визуальный эффект под ногами героя
            Invoke("CastSpell", 1f); //Создание заклинания (spell1)
        }
    }

    void UseSummon() {
        if (tBCastsSpellSummon <= 0 && castSpell == false){
            tBCastsSpellSummon = sTBCastsSpellSummon;
            castSpell = true;
            _animatorController.SetInteger("Switch", 5);
            for (iSummon = 0; iSummon<maxSummon; iSummon++){ //Создаем 2-х гоблинов
                if (iSummon % 2 == 0) spellPos = new Vector3(transform.position.x - 0.3f, transform.position.y - 0.2f, transform.position.z);
                if (iSummon % 2 == 1) spellPos = new Vector3(transform.position.x + 0.2f, transform.position.y - 0.2f, transform.position.z);
                specialEffect.CreatePurpleFlame(spellPos, 2.25f); //Анимация пламени
                Invoke("CastSpellSummon", 2f); //Создание заклинания
            }
        }
    }

    void CastSpell(){
        castSpell = false;
        Instantiate(castingSpell, spellPos, Quaternion.identity);
    }

    void CastSpellSummon(){
        castSpell = false;
        if (iSummon % 2 == 0) spellPos = new Vector3(transform.position.x - 0.3f, transform.position.y - 0.2f, transform.position.z);
        if (iSummon % 2 == 1) spellPos = new Vector3(transform.position.x + 0.2f, transform.position.y - 0.2f, transform.position.z);
        GameObject summonMinion = Instantiate(spellSummon, spellPos, Quaternion.identity);
        summonMinion.GetComponent<Enemy>().SummonOptions(false, tBCastsSpellSummon, "Summon", true);
        //summonMinion.GetComponent<Enemy>().lootOn = false;
        //summonMinion.GetComponent<Enemy>().lifeTime = tBCastsSpellSummon;
        //summonMinion.GetComponent<Enemy>().tag = "Summon";
        //summonMinion.GetComponent<Enemy>().iAmSummon = true;
        iSummon--;
    }

    private void SummonOptions(bool newlootOn, float newtBCastsSpellSummon, string newtag , bool newiAmSummon ) {
        lootOn = newlootOn;
        lifeTime = newtBCastsSpellSummon;
        tag = newtag;
        iAmSummon = newiAmSummon;
    }

    //public void TakeDamage(float damage){ //Возможно стоит удалить
    //    health -= damage;
    //}

    public void KeyDrop(int key){
        keyNum = key;
        /*switch (key){
            case 0:
                name = keyDrop.Brown;
                break;
            case 1:
                name = keyDrop.Green;
                break;
            case 2:
                name = keyDrop.Red;
                break;
            default: break;
        }*/
}

    public void Trigger(bool on) {
        if (on == true && colEnvironment == false){
            colEnvironment = true;
            AILerp.enabled = true;
            Seeker.enabled = true;
            EnemyAIDest.enabled = true;
            //this.GetComponent<Pathfinding.AILerp>().enabled = true;
            //this.GetComponent<Pathfinding.Seeker>().enabled = true;
            //this.GetComponent<Pathfinding.EnemyAIDest>().enabled = true;
        }
        else if (on == false && colEnvironment == true){
            colEnvironment = false;
            AILerp.enabled = false;
            Seeker.enabled = false;
            EnemyAIDest.enabled = false;
            //this.GetComponent<Pathfinding.AILerp>().enabled = false;
            //this.GetComponent<Pathfinding.Seeker>().enabled = false;
            //this.GetComponent<Pathfinding.EnemyAIDest>().enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D col){
        if ((col.gameObject.layer == 10 || col.gameObject.layer == 4 || col.gameObject.layer == 11) && distance < radius){
            Trigger(true);
            // 4 - Water 10 - Env 11 - Hill
        }
    }
}
