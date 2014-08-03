﻿using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
	enum GameState {
		IDLE, 
		OPENING, 
		PAUSE, 
		RUNNING, 
		ENDING, 
		RESULT,
	}
	
	public GameObject gameUILayer;
	public GameObject resultUILayer;
	public GameObject pauseUILayer;

	public GUITexture menuCursor;
	public GUIText scoreText;
	public GUIText MaterialCalorie;
	public GUIText textCombo;  //물체 충돌시 물체 위치에 바로 표시되는 칼로리량 

	public GameModule runningModule;
	public GameModule resultModule;

	public GUIText timer;  //타이머 텍스트 

	public int[] foodIdArray;  //발사할 음식 인덱스 배열 
	public int [] NumberOfCollision;

	public Transform[] projectiles;
	public GUITexture ImagePanel;
	public Transform text_Hit;

	float time;


	private GameObject _foodImageObject;

	private static GameManager instance;

	public static GameManager getInstance() {
		return instance;
	}


	private int currentMaxCombo;
	public int CurrentMaxCombo {
		get { return currentMaxCombo; }
	}
	
	private int currentCombo;
	public int CurrentCombo {
		get { return currentCombo; }
	}
	
	private int currentScore;
	public int CurrentScore {
		get { return currentScore; }
	}
	
	private float currentCalorie;
	public float CurrentCalorie {
		get { return currentCalorie; }
	}

	
	private GameState currentState;


	public bool isGameRunning()
	{
		return (currentState == GameState.RUNNING);
	}


	public bool isGamePause()
	{
		return (currentState == GameState.PAUSE);
	}


	public bool isGameFinished()
	{
		return (currentState == GameState.ENDING) || (currentState == GameState.RESULT);
	}


	public bool isGameResult()
	{
		return (currentState == GameState.RESULT);
	}


	public void showFoodImage(int foodId)
	{
		if (_foodImageObject != null)
			Destroy (_foodImageObject);

		GameObject foodObject = (GameObject)findFoodObject(foodId);

		if (foodObject != null) {
			Transform imageLocation = GameObject.Find ("ImageLocation").transform;

			_foodImageObject = (GameObject)Instantiate (foodObject, imageLocation.position, Quaternion.identity);
			_foodImageObject.rigidbody.useGravity = false;

			_foodImageObject.transform.parent = gameUILayer.transform;
		}
	}
	public void resetCombo()
	{
		textCombo.enabled = false;
		currentCombo = 0;
	}
	public void addCombo()
	{
		currentCombo++;
		textCombo.enabled=true;
		textCombo.text = "+" + currentCombo;

		if(currentCombo > currentMaxCombo)
			currentMaxCombo = currentCombo;
	}
	public void showText(Transform transform,int calorie)
	{
		//text_Hit.transform.guiText.text=calorie.ToString ();

		//textHit.text = calorie.ToString ();
		//Transform text=(Transform) Instantiate(textHit.transform, transform.position, Quaternion.identity);
		Transform text=(Transform) Instantiate(text_Hit, transform.position, Quaternion.identity);
		Destroy (text.gameObject, 0.5f);

	}
	public void foodIdArrayCreate()  //음식인덱스 배열 생성 함수 /// 게임 시작시 음식인덱스를 저장한 배열 생성 
	{

		foodIdArray = new int[150];
		bool reverse = false;
		int j = (int)Random.Range(0,10);
		for (int i=0; i< 150; i++) {
			if(j>10)
			{
				reverse=true;
				j=j-2;
			}
			if(j<0)
			{
				reverse=false;
				j=j+2;
			}
			if(reverse==false)
			{
				foodIdArray[i]=j;
				j++;
			}
			else
			{
				foodIdArray[i]=j;
				j--;
			}

		}
	}

	public void addScore(int score)
	{
		currentScore += score;
	}


	public void addMovingDistance(float distance)
	{
		currentCalorie += getCalorieFromDistance(distance);
	}


	public float getCalorieFromDistance(float distance)
	{
		// 임시로 가정된 수치로 계산함

		const float K = 0.000325f;
		const float U = 0.1615f;

		float calorie = 0.0f;
		calorie = ((distance / U) * K);

		return calorie;
	}


	public void showScore(int score)
	{
		if (MaterialCalorie != null)
			MaterialCalorie.text = score.ToString();
	}


	public void CheckFoodCrash(int FoodID)
	{
		NumberOfCollision [FoodID] += 1;


	}


	int getMostCollisionFood()
	{
		int max;
		int maxNum=0;
		max=NumberOfCollision[0];
		for(int i=0; i<10; ++i)
		{
			if(max < NumberOfCollision[i])
			{
				max=NumberOfCollision[i];
				maxNum=i;
			}
		}
		return maxNum;
	}


	int getLeastCollisionFood()
	{
		int min;
		int minNum=0;
		min=NumberOfCollision[0];
		for(int i=0; i<10; ++i)
		{
			if(min > NumberOfCollision[i])
			{
				min=NumberOfCollision[i];
				minNum=i;
			}
		}
		return minNum;
	}

	public void restart()
	{
		restartGame();
	}


	private GameObject findFoodObject(int foodId)
	{
		for (int i = 0; i < projectiles.Length; ++i) {
			CalorieFoodObject currentObject = projectiles[i].GetComponent<CalorieFoodObject>();

			if (currentObject.foodId == foodId) {
				return projectiles[i].gameObject;
			}
		}
		return null;
	}


	void init()
	{
		textCombo.enabled = false;
		instance = this;
	}


	/// <summary>
	/// 게임 데이터를 초기화한다.
	/// </summary>
	void initGameData()
	{
		NumberOfCollision = new int[projectiles.Length];
		for (int i=0; i<projectiles.Length; i++) 
			NumberOfCollision[i]=0;

		currentCalorie = 0.0f;
		currentCombo = 0;
		currentMaxCombo = 0;
		time = 0.0f;
		
		ProjectileThrower.getInstance ().InitK();
		timer.color = Color.white;
		timer.fontSize = 32;
	}

	
	void Start () 
	{
	
		init();

		prepareGame();
		foodIdArrayCreate();
		startGame();

	    checkKinectCalibration();   
	}


	void Update ()
	{
		checkKinectCalibration();

		if (currentState == GameState.RUNNING) {
			timerUI ();
			updateScoreText();
			if(time>=20)
			{	timer.color=Color.red;
				timer.fontSize=34;

			}
			if (time>=30) {
				finishGame ();
			}
		}

	}


	void OnDisable()
	{
		instance = null;
	}
	void timerUI()
	{
		time += Time.deltaTime;
		string timeStr;
		timeStr = "" + time.ToString ("00.00");
		//timeStr = timeStr.Replace (".", ":");

		int a = int.Parse(timeStr.Substring (0, 2));
		int b = int.Parse(timeStr.Substring(3,2));
		int time1 = 59 - a;
		int time2 = 99 - b;
		timeStr = time1.ToString("00") + ":" + time2.ToString("00");
		timer.text = timeStr;
	}


	/// <summary>
	/// 키넥트에서 사용자를 인식 중인지 확인하여, 아닐 경우 Pause 상태로 만든다.
	/// </summary>
	void checkKinectCalibration()
	{
		if (currentState == GameState.RUNNING || currentState == GameState.PAUSE) {
			KinectManager kinectManager = KinectManager.Instance;
			if (kinectManager == null)
				return;
			
			bool isUserDetected = KinectManager.Instance.IsUserDetected();

			if (isUserDetected) {
				resumeGame();
			} else {
				pauseGame();
			}
		}
	}


	void prepareGame()
	{
		updateState(GameState.IDLE);
	}


	void startGame()
	{
		updateState(GameState.OPENING);
	}


	void pauseGame()
	{
		updateState(GameState.PAUSE);
	}


	void resumeGame()
	{
		updateState(GameState.RUNNING);

	}

	
	void finishGame()
	{
		updateState(GameState.ENDING);
	}


	void restartGame()
	{
		prepareGame();
		startGame();
	}


	IEnumerator runOpeningAction(int actionIndex)
	{
		float delay = 1.0f;
		bool finished = false;

		switch (actionIndex) {
		case 0:
			break;

		case 1:
			finished = true;
			break;
		}

		if (finished) {
			onOpeningFinished();
			yield break;
		}

		else {
			yield return new WaitForSeconds(delay);
			StartCoroutine("runOpeningAction", ++actionIndex);
		}
	}


	IEnumerator runEndingAction(int actionIndex)
	{
		float delay = 1.0f;
		bool finished = false;
		
		switch (actionIndex) {
		case 0:
			break;
			
		case 1:
			finished = true;
			break;
		}
		
		if (finished) {
			onEndingFinished();
			yield break;
		}
		
		else {
			yield return new WaitForSeconds(delay);
			StartCoroutine("runEndingAction", ++actionIndex);
		}
	}


	private void playOpening()
	{
		int actionIndex = 0;
		StartCoroutine("runOpeningAction", actionIndex);
	}


	private void playEnding()
	{
		int actionIndex = 0;
		StartCoroutine("runEndingAction", actionIndex);
	}
	

	private void onOpeningFinished()
	{
		updateState(GameState.RUNNING);
	}


	private void onEndingFinished()
	{
		updateState(GameState.RESULT);
	}
	

	void updateState(GameState nextState)
	{
		switch (nextState) {
		case GameState.IDLE:
			initGameData();
			break;
			
		case GameState.OPENING:
			if (currentState != GameState.IDLE)
				return;
			
			playOpening();
			break;
			
		case GameState.RUNNING:
			if (currentState == GameState.PAUSE) {
				Time.timeScale = 1;
			}
			
			RenderSettings.ambientLight = Color.white;
			break;
			
		case GameState.PAUSE:
			Time.timeScale = 0;
			RenderSettings.ambientLight = Color.black;
			break;
			
		case GameState.ENDING:
			playEnding();
			break;
			
		case GameState.RESULT:
			RenderSettings.ambientLight = Color.black;
			startResultModule();
			break;
		}
		updateUI(nextState);
		
		currentState = nextState;
	}


	void updateUI(GameState nextState)
	{
		gameUILayer.SetActive(false);
		resultUILayer.SetActive(false);
		pauseUILayer.SetActive(false);
		
		menuCursor.enabled = false;
		
		switch (nextState) {
		case GameState.OPENING:
		case GameState.ENDING:
			break;
			
		case GameState.IDLE:
			if (_foodImageObject != null) {
				DestroyObject(_foodImageObject);
			}
			break;
			
		case GameState.PAUSE:
			pauseUILayer.SetActive(true);
			break;
			
		case GameState.RUNNING:
			gameUILayer.SetActive(true);
			break;
			
		case GameState.RESULT:
			resultUILayer.SetActive(true);
			menuCursor.enabled = true;
			break;
		}
	}


	void updateScoreText()
	{
		if (scoreText != null)
			scoreText.text = currentScore.ToString();
	}


	private void startResultModule()
	{
		if (resultModule == null)
			return;
		
		Hashtable paramTable = new Hashtable();
		
		int bestFoodIndex = getMostCollisionFood();
		int worstFoodIndex = getLeastCollisionFood();
		
		int grade = calculateGrade(currentScore, currentCalorie);
		string gradeText = getGradeText(grade);
	
		// 소수점 두 번째 자리까지 표시
		float roundedCalorie = Mathf.Round(currentCalorie * 100) / 100;

		paramTable.Add("grade", gradeText);
		paramTable.Add("score", currentScore);
		paramTable.Add("calorie", roundedCalorie);
		paramTable.Add("max_combo", currentMaxCombo);
		paramTable.Add("best_food", bestFoodIndex);
		paramTable.Add("worst_food", worstFoodIndex);
		
		resultModule.startModule(paramTable);
	}


	private int calculateGrade(int score, float calorie)
	{
		// 임시로 등급을 SSS, SS, S, A, B, C, D, F 8단계로 구분
		return (score / 1000);
	}
	
	
	private string getGradeText(int grade)
	{
		string[] GRADE_TEXT = {
			"F", "D", "C", "B", "A", "S", "SS", "SSS",
		};

		if (grade > GRADE_TEXT.Length)
			grade = GRADE_TEXT.Length;

		return GRADE_TEXT[grade];
	}
}
