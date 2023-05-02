package firstTry;

import com.intel.crypto.Random;
import com.intel.util.*;
import com.intel.langutil.ArrayUtils;


//
// Implementation of DAL Trusted Application: TempProject 
//
// **************************************************************************************************
// NOTE:  This default Trusted Application implementation is intended for DAL API Level 7 and above
// **************************************************************************************************

public class Password1 extends IntelApplet {
	final int CMD_CHECK_MAIN_PASSWORD =0;
	final int CMD_GET_PASSWORD = 3;
	final int CMD_GENERATE_PASSWORD = 1;
	final int CMD_SAVE_PASSWORD = 2;
	final int CMD_MAKE_NEW_MAIN_PASSWORD=4;

	public static final short PASSWORD_SIZE = 8;
	public static final short USER_NAME_SIZE = 24;
	public static final short URL_SIZE = 224;
	public static final short FULL_PASSWORD_SIZE =PASSWORD_SIZE + USER_NAME_SIZE + URL_SIZE;
	public static final short SIZE_OF_PASSWORD_REQUIRMENTS = 3;
	public static final short THREE_RAND_SIZE = 120;
	
	public static Boolean is_logged = false;
	
    private static final char[] LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".toCharArray();
    private static final char[] SPECIAL_CHARACTERS = "!@#$%^&*()_-+=".toCharArray();
    private static final char[] NUMBERS = "0123456789".toCharArray();

	/**
	 * This method will be called by the VM when a new session is opened to the Trusted Application 
	 * and this Trusted Application instance is being created to handle the new session.
	 * This method cannot provide response data and therefore calling
	 * setResponse or setResponseCode methods from it will throw a NullPointerException.
	 * 
	 * @param	request	the input data sent to the Trusted Application during session creation
	 * 
	 * @return	APPLET_SUCCESS if the operation was processed successfully, 
	 * 		any other error status code otherwise (note that all error codes will be
	 * 		treated similarly by the VM by sending "cancel" error code to the SW application).
	 */
	public int onInit(byte[] request) {
		DebugPrint.printString("Hello, DAL!");
		return APPLET_SUCCESS;
	}
	
	/**
	 * This method will be called by the VM to handle a command sent to this
	 * Trusted Application instance.
	 * 
	 * @param	commandId	the command ID (Trusted Application specific) 
	 * @param	request		the input data for this command 
	 * @return	the return value should not be used by the applet
	 */
	public int invokeCommand(int commandId, byte[] request) {
		byte[] myResponse=null;
		int respCode=0;
		DebugPrint.printString("Received command Id: " + commandId + ".");
		if(request != null)
		{
			DebugPrint.printString("Received buffer:");
			DebugPrint.printBuffer(request);
		}
		switch(commandId)
		{
		case CMD_MAKE_NEW_MAIN_PASSWORD:
		{
			try {
		          makeNewMainPassword(request);
		          respCode=0;
		        }
			catch(Exception e) 
			   { 
				if(e.getMessage()=="Password is incorrect length!")
					respCode=1;
				else if(e.getMessage()=="password was not saved!")
					respCode=1;
				else if(e.getMessage()=="User is already registered!")
					respCode=2;
				else
					respCode=1;
		       }
		   break;
		}
		case CMD_CHECK_MAIN_PASSWORD:
		{
			try {
				if(checkMainPassword(request)==true)
					respCode=0;
		        }
			catch(Exception e) 
			   { 
				if(e.getMessage()=="incorrect password entered!")
					respCode=1;
				else if(e.getMessage()=="User is already logged in!")
					respCode=0;
				else if(e.getMessage()=="There is no user registered in the system!")
					respCode=2;
				else
					respCode=2;
		       }
			break;
		}
		case CMD_SAVE_PASSWORD:
		{
			try
			{	
				if(savePassword(request)==true)
					respCode=0;
				else
					respCode=1;
			}
			catch(Exception e)
			{   
				respCode=1;
			}
			break;
		}
		case CMD_GET_PASSWORD:
		{
			try {
				DebugPrint.printString("b:");
				myResponse=getPassword(request);
				respCode=0;
				DebugPrint.printString("bb:");
				}
			catch(Exception e) 
			{
				if(e.getMessage()=="not found")
					respCode=1;
				else
					respCode=2;
			}
			break;
		}
		case CMD_GENERATE_PASSWORD:
			try
			{
				myResponse = generatePassword();
				DebugPrint.printString("Received buffer:");
				DebugPrint.printBuffer(myResponse);
				respCode=0;
				
			}
			catch(Exception e)
			{
				respCode=1;
			}
			break;
		default:
			respCode=4;
		}
		DebugPrint.printString("b:");
		if(myResponse==null)
			myResponse =new byte[]{1};
		//final byte[] myResponse = { 'O', 'K' };

		/*
		 * To return the response data to the command, call the setResponse
		 * method before returning from this method. 
		 * Note that calling this method more than once will 
		 * reset the response data previously set.
		 */
		DebugPrint.printString("Return buffer:");
		DebugPrint.printBuffer(myResponse);
		setResponse(myResponse, 0, myResponse.length);

		/*
		 * In order to provide a return value for the command, which will be
		 * delivered to the SW application communicating with the Trusted Application,
		 * setResponseCode method should be called. 
		 * Note that calling this method more than once will reset the code previously set. 
		 * If not set, the default response code that will be returned to SW application is 0.
		 */
		setResponseCode(respCode);

		/*
		 * The return value of the invokeCommand method is not guaranteed to be
		 * delivered to the SW application, and therefore should not be used for
		 * this purpose. Trusted Application is expected to return APPLET_SUCCESS code 
		 * from this method and use the setResposeCode method instead.
		 */
		return APPLET_SUCCESS;
	}
	
	/**
	 * get new main password and save it in memory
	 * @param password
	 * @throws Exception
	 */
	public void makeNewMainPassword(byte[] password) throws Exception
	{  
		//check length of password is 8 characters
		if(password.length!=8)
			throw new Exception("Password is incorrect length!");
		//if user already registered
		if(FlashStorage.getFlashDataSize(0)!=0)
			throw new Exception("User is already registered!");
		FlashStorage.writeFlashData(0, password, 0, password.length);
		//check that password was saved
		if(FlashStorage.getFlashDataSize(0)!=0)
		{
			DebugPrint.printString("password was saved succesfully!");
			//save empty data in the beginning of memory 1
			byte[] empty=new byte[FULL_PASSWORD_SIZE];
			FlashStorage.writeFlashData(1, empty, 0, FULL_PASSWORD_SIZE);
		}
		else
		{
			DebugPrint.printString("password was not saved!");
			throw new Exception("password was not saved!");
		}		
	}
	
	/**
	 * check if the password the user entered is the correct password
	 * @param my_password the password the user entered
	 * @return true if the password is correct
	 * @throws Exception
	 */
	Boolean checkMainPassword(byte[] my_password) throws Exception
	{
		//if already logged in
		if(is_logged)
			throw new Exception("User is already logged in!");
				
		//if no user is registered
		if(FlashStorage.getFlashDataSize(0)==0)
			throw new Exception("There is no user registered in the system!");
				
		byte[] saved_password=new byte[8];
		FlashStorage.readFlashData(0,saved_password, 0);
		DebugPrint.printString("Main password:");
		DebugPrint.printBuffer(saved_password);
			    
		//if incorrect password was entered
		//check length of password is 8 characters
		if(my_password.length!=8)
			throw new Exception("incorrect password entered!");
		for(int i=0;i<8;i++)
		{
			if(saved_password[i]!=my_password[i])
				throw new Exception("incorrect password entered!");
		}
				
		is_logged=true;
		DebugPrint.printString("logged in");
		return true;
	}
	

	
	/**
	 * gets url and return the password of the url if the url not exists throws exception
	 * @param url_array
	 * @return password and user name array
	 */
	byte[] getPassword(byte[] url_array) throws Exception
	{
		//check if the user logged in
		if(is_logged==false)
			throw new Exception("the user is not logged in");
		if(url_array.length!=URL_SIZE)
			throw new Exception("wrong size of url");
		int memSize=FlashStorage.getFlashDataSize(1);
		DebugPrint.printInt(memSize);
		DebugPrint.printInt(FlashStorage.getFlashDataSize(1));
		//if the data is not empty
		if(memSize!=0)
		{
			byte[] data = new byte[memSize];
			byte[] user_name_password = new byte[PASSWORD_SIZE + USER_NAME_SIZE];
			FlashStorage.readFlashData(1,data,0);
			for(int i=0; i<memSize; i+=FULL_PASSWORD_SIZE)
			{
				DebugPrint.printInt(i);
				if(ArrayUtils.findInByteArray(url_array, 0, url_array.length, data, i, URL_SIZE) != -1)
				{
					DebugPrint.printInt(i);
					ArrayUtils.copyByteArray(data, i+URL_SIZE, user_name_password, 0, PASSWORD_SIZE + USER_NAME_SIZE);
					return user_name_password;
				}
			}
			throw new Exception("not found");
		}
		else
		{
			DebugPrint.printString("ERROR: cant get password!");
			throw new Exception("Error");
		}
	}
	
	/**
	 * gets a url byte array and return the index of the password if this url has a password in memory else return 0
	 * @param url
	 * @return
	 */
	int isUrlExists(byte[] url_array)
	{
		int memSize=FlashStorage.getFlashDataSize(1);
		if(memSize!=0)
		{
			byte[] data = new byte[memSize];
			FlashStorage.readFlashData(1,data,0);
			for(int i=0; i<memSize; i+=FULL_PASSWORD_SIZE)
			{
				DebugPrint.printInt(i);
				if(ArrayUtils.findInByteArray(url_array, 0, url_array.length, data, i, URL_SIZE) != -1)
				{
					return i;
				}
			}
			return 0;
		}
		return 0;
	}
	
	/**
	 * I think that need to add check if the url exists if yes update if no add!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	 * the format of the memory is in memory 1 [url*224, password*8, user name*24 ]
	 * gets byte array of url, password,user name and check if the url is already exists
	 * if not save it in memory
	 * @param password_userName_url_array
	 * @return
	 * @throws Exception
	 */
	Boolean savePassword(byte[] password_userName_url_array) throws Exception
	{
		//check if the user logged in
		if(is_logged==false)
			throw new Exception("the user is not logged in");
		//check if the array is the correct size
		if(password_userName_url_array.length != FULL_PASSWORD_SIZE)
			throw new Exception("inncorect size");
		byte[] url=new byte[URL_SIZE];
		ArrayUtils.copyByteArray(password_userName_url_array, 0, url, 0, URL_SIZE);
		int memSize=FlashStorage.getFlashDataSize(1);
		DebugPrint.printInt(memSize);
		DebugPrint.printInt(FlashStorage.getFlashDataSize(1));
		//check if the pointer to the next free space in memory is in the right place
		if(memSize!=0)
		{
			//check if the url exists
			if(isUrlExists(url)!=0)
			{
				int index=isUrlExists(url);
				//make an array that will contain the data from memory
				byte[] data1 = new byte[memSize];
				//read data from memory and put it in the array
				FlashStorage.readFlashData(1,data1,0);
				DebugPrint.printString("origin data:");
				DebugPrint.printBuffer(data1);
				
				//change the array from index to the password was send
				ArrayUtils.copyByteArray(password_userName_url_array, 0, data1, index, FULL_PASSWORD_SIZE);
				DebugPrint.printString("new data1:");
				DebugPrint.printBuffer(data1);
				//write all the data to memory old and new
				FlashStorage.writeFlashData(1, data1, 0, data1.length);
				
				return true;
			}
			//make an array that will contain the data from memory+the new data
			byte[] data = new byte[memSize + FULL_PASSWORD_SIZE];
			//read data from memory and put it in the array
			FlashStorage.readFlashData(1,data,0);
			//add to the end of the array the new password
			ArrayUtils.copyByteArray(password_userName_url_array, 0, data, memSize, FULL_PASSWORD_SIZE);
			//write all the data to memory old and new
			FlashStorage.writeFlashData(1, data, 0, data.length);
		}
		else
		{
			//the next free space in memory is not in the right place
			throw new Exception("error");
		}
		memSize+= FULL_PASSWORD_SIZE;
		DebugPrint.printInt(memSize);
		DebugPrint.printInt(FlashStorage.getFlashDataSize(1));
		//check that password was saved
		if(FlashStorage.getFlashDataSize(1)==memSize)
		{
			DebugPrint.printString("url, password and username was saved succesfully!");
		}
		else
		{
			DebugPrint.printString("ERROR: not saved!");
			return false;
		}
		return true;
	}
	

	/**
	 * make a random password size 8 which contains
	 * one special letter, one number and the rest small or capital letters
	 * @return the password
	 */
	byte[] generatePassword() throws Exception
	{
		//check if the user logged in
		if(is_logged==false)
			throw new Exception("the user is not logged in");
        byte[] randomArray = new byte[PASSWORD_SIZE];
        Random.getRandomBytes(randomArray,(short)0,PASSWORD_SIZE);
        // Get the index to place special character
        int specialIndex = randomArray[0] % PASSWORD_SIZE;
        
        // Get the index to place number
        int numberIndex = (randomArray[1] & 0xFF) % PASSWORD_SIZE;
        
        byte[] passwordBytes = new byte[PASSWORD_SIZE];
        
        // Place special character in random position
        passwordBytes[specialIndex] = (byte) SPECIAL_CHARACTERS[((randomArray[2] < 0 ? ((randomArray[2] ^ -1) + 1) & 0xFF : randomArray[2]) % SPECIAL_CHARACTERS.length)];
        
        // Place number in random position
        passwordBytes[numberIndex] = (byte) NUMBERS[((randomArray[3] < 0 ? ((randomArray[3] ^ -1) + 1) & 0xFF : randomArray[3]) % NUMBERS.length)];

        // Place random letters in remaining positions
        int letterIndex = 0;
        for (int i = 0; i < 8; i++) {
            if (i == specialIndex || i == numberIndex) {
                continue;
            }
            passwordBytes[i] = (byte) LETTERS[((randomArray[letterIndex] < 0 ? ((randomArray[letterIndex] ^ -1) + 1) & 0xFF : randomArray[letterIndex]) % LETTERS.length)];
            letterIndex++;
        }
		DebugPrint.printString("password generated:");
		DebugPrint.printBuffer(passwordBytes);
		return passwordBytes;
	}
	
	/**
	 * This method will be called by the VM when the session being handled by
	 * this Trusted Application instance is being closed 
	 * and this Trusted Application instance is about to be removed.
	 * This method cannot provide response data and therefore
	 * calling setResponse or setResponseCode methods from it will throw a NullPointerException.
	 * 
	 * @return APPLET_SUCCESS code (the status code is not used by the VM).
	 */
	public int onClose() {
		DebugPrint.printString("Goodbye, DAL!");
		return APPLET_SUCCESS;
	}
}
