using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boringCheckers
{
    class Program
    {
        struct piece {
            public bool red; //bool to hold if piece is red or black true means red
            public int row; //int to hold the row position of the piece in the 2d array
            public int col; //int to hold the col position of the piece in the 2d array
            public bool king; //bool to hold whether or not the piece has been kinged
            public bool inPlay; //bool to keep track of whether or not the piece is in play
        }// end struct
        
        static void Main(string[] args)
        {
            rules(); //displaying the rules using rules function
            int[,] board = new int[8,8]; //creating a 2d array to be used as the board
            initializeBoard(board); //initializing 2d int array to 0 meaning a black square or -1 meaning a red (unplayable) square
            piece[] redPieces = new piece[12]; //creating the array of 12 pieces for the red pieces
            piece[] blackPieces = new piece[12]; //creating the array of 12 pieces for the black pieces
            startPieces(redPieces, true, board); //calling startPieces to initialize the values in the piece array and to set the pieces on the board
            startPieces(blackPieces, false, board); //calling startPieces to initialize the values in the piece array and to set the pieces on the board
            int redCount = getCount(redPieces); //getting the count of red Pieces in play (will always be 12 because the game has not begun)
            int blackCount = getCount(blackPieces); //getting the count of black Pieces in play (will always be 12 because the game has not begun)
            drawBoard(board, redCount, blackCount); //drawing the intialized board and pieces to the screen
            int turnCount = 0; //creating an int to alternate player turns

            //game keeps playing until the win condition is met
            //win condition is either red player or black player has no pieces left
            while (redCount > 0 && blackCount > 0){
                turnCount++; //starting the turn count to alternate player turns
                bool done = false; //creating the bool to allow the player to keep entering in new moves until a valid  move is entered
                bool again = false; //creating bool to create a way to double jump
                int selectedPiece = -1; //initializing selected piece to "no index found" or -1
                Console.SetCursorPosition(0,0); //setting the cursor back at 0,0

                //if turncount mod 2 results in odd then it is player one's turn
                //else it is player two's turn
                if (turnCount%2 == 1){
                    //this while loop allows the player to keep entering moves until a valid move is entered
                    while (!done){
                        //prompting the user to select the location of the piece they want to move
                        //that input is then converted into the cooresponding index in the 2d int array
                        string input = Prompt("Player One please select your piece");
                        int[] piecePos = getPosition(input);
                        //prompting user to select the location on the board they would like to move the piece
                        //that input is then converted into the cooresponding index in the 2d int array
                        input = Prompt("Please select where you would like to move your piece");
                        int [] newPos = getPosition(input);
                        //these are a few math calculations that are needed in order to determine if the entered move is valid
                        //rowABS is the absolute value of the newPos row subtracted from the piecePos row
                        //colABS is the absolute value of the newPos col subtracted from the piecePos col
                        decimal rowAbs = Math.Abs((decimal)(newPos[0] - piecePos[0]));
                        decimal colAbs = Math.Abs((decimal)(newPos[1] - piecePos[1]));
                        //these mids are used to determine the position of the square jumped when a player jumps another piece
                        //mids are used as the jumped square index, then they are put in an int array as usable by verify piece
                        int colMid = (newPos[1] + piecePos[1]) / 2; 
                        int rowMid = (newPos[0] + piecePos[0]) / 2;
                        int[] midPos = {rowMid, colMid};
                        //getting the index of the selectedPiece in the corresponding piece array
                        selectedPiece = verifyPiece(redPieces, piecePos, board);

                        //this is the greedy algorithm that rules out all invalid moves for the selected piece
                        if (selectedPiece < 0){ //if the selected piece is a red (invalid) square
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if ((piecePos[0] < 0 || piecePos[0] > 7) || (piecePos[1] < 0 || piecePos[1] > 7)){ //if the selected piece index is outside of bound of the board, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if ((newPos[0] < 0 || newPos[0] > 7) || (newPos[1] < 0 || newPos[1] > 7)){ //if the new position index is outside of bound of the board, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if (board[newPos[0], newPos[1]] != 0){ //if the selected new position is not an empty black square, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if (redPieces[selectedPiece].king == false && newPos[0] <= piecePos[0]){ //if the piece is not a king and they are trying to move backwards, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if (rowAbs > 2 || colAbs > 2){ //if the new position is more than 2 spaces away from the original, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if (!(rowAbs == colAbs)){ //if the new position is not a square in a diagonal line from the original, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if ((rowAbs == 2 && colAbs == 2) && (board[rowMid, colMid] % 2 == 1 || board[rowMid, colMid] == 0)){ //if a jump is attempted and the jump is not over an opposite
                            Console.WriteLine("Invalid move, try again.");                                                         //player's piece, then invalid move
                            pause();
                        }else if ((rowAbs == 2 && colAbs == 2) && board[rowMid, colMid] % 2 == 0){ //if a jump is attempted and good
                            redPieces[selectedPiece] = movePiece(redPieces[selectedPiece], newPos, board);//then the selected piece is moved, the jumped piece is removed, and the count is updated
                            int tempPiece = verifyPiece(blackPieces, midPos, board);                      //again is set to true to give the option for a double jump
                            blackPieces[tempPiece] = removePiece(blackPieces[tempPiece], board);
                            blackCount = getCount(blackPieces);
                            again = true;
                        }else{ //else the player moved one space forward and the selected piece is moved and the turn is ended
                            redPieces[selectedPiece] = movePiece(redPieces[selectedPiece], newPos, board);
                            done = true;
                        }
                        //check for a king after any move
                        kingCheck(redPieces, board);
                        int tempCount = 1; //creating a temp counter to keep track of how many jumps the player has used
                        //clearing console, redrawing the board, then resetting the cursor to 0,0
                        Console.Clear();
                        drawBoard(board, redCount, blackCount);
                        Console.SetCursorPosition(0,0);
                        //a while loop that loops until the player ends their turn by entering 'q'
                        //this loop allows the player to double jump
                        while (again){
                            //incrementing tempCount to display how many jumps the player has done
                            tempCount++;
                            //displaying instructions for the user then prompting for the next jump/quit command
                            Console.WriteLine("If you have another jump you may take that now. Enter 'q' to end turn.");
                            input = Prompt("Please enter the position for jump " + tempCount);
                            //checking for quit command and adjusting bools appropriately
                            if (input.Contains('q')){
                                again = false;
                                done = true;
                            }
                            //if no quit command then try the jump position
                            if (again){
                                //resetting the new position from input and resetting piecePos of the previously selected piece
                                //recalculating the math for the absolute value from the jump to find the jumped square
                                newPos = getPosition(input);
                                piecePos[0] = redPieces[selectedPiece].row;
                                piecePos[1] = redPieces[selectedPiece].col;
                                rowAbs = Math.Abs((decimal)(newPos[0] - piecePos[0]));
                                colAbs = Math.Abs((decimal)(newPos[1] - piecePos[1]));
                                colMid = (newPos[1] + piecePos[1]) / 2;
                                rowMid = (newPos[0] + piecePos[0]) / 2;
                                midPos[0] = rowMid;
                                midPos[1] = colMid;
                                if ((newPos[0] < 0 && newPos[0] > 7) || (newPos[1] < 0 && newPos[1] > 7)){ //if the new position index is outside of bound of the board, then invalid move
                                    Console.WriteLine("Invalid move, try again.");
                                    pause();
                                }else if (board[newPos[0], newPos[1]] != 0){ //if the selected new position is not an empty black square, then invalid move
                                    Console.WriteLine("Invalid move, try again.");
                                    pause();
                                }else if (redPieces[selectedPiece].king == false && newPos[0] <= piecePos[0]){ //if the piece is not a king and they are trying to move backwards, then invalid move
                                    Console.WriteLine("Invalid move, try again.");
                                    pause();
                                }else if ((rowAbs == 2 && colAbs == 2) && (board[rowMid, colMid] % 2 == 1 || board[rowMid, colMid] == 0)){ //if the move is a jump but the jumpped piece is not
                                    Console.WriteLine("Invalid move, try again.");                                                         //the opposite player's, then invalid move
                                    pause();
                                }else if (!(rowAbs == 2 && colAbs == 2) && board[rowMid, colMid] % 2 == 0){ //if the move is anything other than a jump over the opposite player's piece, then invalid move
                                    Console.WriteLine("Invalid move, try again.");
                                    pause();
                                }else{ //else the move is a jump over the opposite player's piece, so execute
                                    redPieces[selectedPiece] = movePiece(redPieces[selectedPiece], newPos, board);//move the selected piece
                                    int tempPiece = verifyPiece(blackPieces, midPos, board);
                                    blackPieces[tempPiece] = removePiece(blackPieces[tempPiece], board);//remove the jumped piece
                                    blackCount = getCount(blackPieces);//recalculate the piece count
                                }
                                //check for king after every move
                                kingCheck(redPieces, board);
                                //clearing console, redrawing the board, then resetting the cursor to 0,0
                                Console.Clear();
                                drawBoard(board, redCount, blackCount);
                                Console.SetCursorPosition(0,0);
                            }
                        }
                    }
                //else it is player two's turn
                }else{
                    //this while loop allows the player to keep entering moves until a valid move is entered
                    while (!done){
                        //prompting the user to select the location of the piece they want to move
                        //that input is then converted into the cooresponding index in the 2d int array
                        string input = Prompt("Player Two please select your piece");
                        int[] piecePos = getPosition(input);
                        //prompting user to select the location on the board they would like to move the piece
                        //that input is then converted into the cooresponding index in the 2d int array
                        input = Prompt("Please select where you would like to move your piece");
                        int[] newPos = getPosition(input);
                        //these are a few math calculations that are needed in order to determine if the entered move is valid
                        //rowABS is the absolute value of the newPos row subtracted from the piecePos row
                        //colABS is the absolute value of the newPos col subtracted from the piecePos col
                        decimal rowAbs = Math.Abs((decimal)(newPos[0] - piecePos[0]));
                        decimal colAbs = Math.Abs((decimal)(newPos[1] - piecePos[1]));
                        //these mids are used to determine the position of the square jumped when a player jumps another piece
                        //mids are used as the jumped square index, then they are put in an int array as usable by verify piece
                        int colMid = (newPos[1] + piecePos[1]) / 2; 
                        int rowMid = (newPos[0] + piecePos[0]) / 2;
                        int[] midPos = {rowMid, colMid};
                        //getting the index of the selectedPiece in the corresponding piece array
                        selectedPiece = verifyPiece(blackPieces, piecePos, board);

                        //this is the greedy algorithm that rules out all invalid moves for the selected piece
                        if (selectedPiece < 0){ //if the selected piece is a red (invalid) square
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if ((piecePos[0] < 0 && piecePos[0] > 7) || (piecePos[1] < 0 && piecePos[1] > 7)){ //if the selected piece index is outside of bound of the board, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if ((newPos[0] < 0 && newPos[0] > 7) || (newPos[1] < 0 && newPos[1] > 7)){ //if the new position index is outside of bound of the board, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if (board[newPos[0], newPos[1]] != 0){ //if the selected new position is not an empty black square, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if (blackPieces[selectedPiece].king == false && newPos[0] >= piecePos[0]){ //if the piece is not a king and they are trying to move backwards, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if (rowAbs > 2 || colAbs > 2){ //if the new position is more than 2 spaces away from the original, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if (!(rowAbs == colAbs)){ //if the new position is not a square in a diagonal line from the original, then invalid move
                            Console.WriteLine("Invalid move, try again.");
                            pause();
                        }else if ((rowAbs == 2 && colAbs == 2) && (board[rowMid, colMid] % 2 == 0 || board[rowMid, colMid] == 0)){ //if a jump is attempted and the jump is not over an opposite
                            Console.WriteLine("Invalid move, try again.");                                                         //player's piece, then invalid move
                            pause();
                        }else if ((rowAbs == 2 && colAbs == 2) && board[rowMid, colMid] % 2 == 1){ //if a jump is attempted and good
                            blackPieces[selectedPiece] = movePiece(blackPieces[selectedPiece], newPos, board);//then the selected piece is moved, the jumped piece is removed, and the count is updated
                            int tempPiece = verifyPiece(redPieces, midPos, board);                            //again is set to true to give the option for a double jump
                            redPieces[tempPiece] = removePiece(redPieces[tempPiece], board);
                            redCount = getCount(redPieces);
                            again = true;
                        }else{ //else the player moved one space forward and the selected piece is moved and the turn is ended
                            blackPieces[selectedPiece] = movePiece(blackPieces[selectedPiece], newPos, board);
                            done = true;
                        }
                        //check for a king after any move
                        kingCheck(blackPieces, board);
                        int tempCount = 1; //creating a temp counter to keep track of how many jumps the player has used
                        //clearing console, redrawing the board, then resetting the cursor to 0,0
                        Console.Clear();
                        drawBoard(board, redCount, blackCount);
                        Console.SetCursorPosition(0,0);
                        //a while loop that loops until the player ends their turn by entering 'q'
                        //this loop allows the player to double jump
                        while (again){
                            //incrementing tempCount to display how many jumps the player has done
                            tempCount++;
                            //displaying instructions for the user then prompting for the next jump/quit command
                            Console.WriteLine("If you have another jump you may take that now. Enter 'q' to end turn.");
                            input = Prompt("Please enter the position for jump " + tempCount);
                            //checking for quit command and adjusting bools appropriately
                            if (input.Contains('q')){
                                again = false;
                                done = true;
                            }
                            //if no quit command then try the jump position
                            if (again){
                                //resetting the new position from input and resetting piecePos of the previously selected piece
                                //recalculating the math for the absolute value from the jump to find the jumped square
                                newPos = getPosition(input);
                                piecePos[0] = blackPieces[selectedPiece].row;
                                piecePos[1] = blackPieces[selectedPiece].col;
                                rowAbs = Math.Abs((decimal)(newPos[0] - piecePos[0]));
                                colAbs = Math.Abs((decimal)(newPos[1] - piecePos[1]));
                                colMid = (newPos[1] + piecePos[1]) / 2;
                                rowMid = (newPos[0] + piecePos[0]) / 2;
                                midPos[0] = rowMid;
                                midPos[1] = colMid;
                                if ((newPos[0] < 0 && newPos[0] > 7) || (newPos[1] < 0 && newPos[1] > 7)){ //if the new position index is outside of bound of the board, then invalid move
                                    Console.WriteLine("Invalid move, try again.");
                                    pause();
                                }else if (board[newPos[0], newPos[1]] != 0){ //if the selected new position is not an empty black square, then invalid move
                                    Console.WriteLine("Invalid move, try again.");
                                    pause();
                                }else if (blackPieces[selectedPiece].king == false && newPos[0] >= piecePos[0]){ //if the piece is not a king and they are trying to move backwards, then invalid move
                                    Console.WriteLine("Invalid move, try again.");
                                    pause();
                                }else if ((rowAbs == 2 && colAbs == 2) && (board[rowMid, colMid] % 2 == 0 || board[rowMid, colMid] == 0)){ //if the move is a jump but the jumpped piece is not
                                    Console.WriteLine("Invalid move, try again.");                                                         //the opposite player's, then invalid move
                                    pause();
                                }else if (!((rowAbs == 2 && colAbs == 2) && board[rowMid, colMid] % 2 == 1)){ //if the move is anything other than a jump over the opposite player's 
                                    Console.WriteLine("Invalid move, try again.");                            //piece, then invalid move
                                    pause();
                                }else{ //else the move is a jump over the opposite player's piece, so execute
                                    blackPieces[selectedPiece] = movePiece(blackPieces[selectedPiece], newPos, board); //move the selected piece
                                    int tempPiece = verifyPiece(redPieces, midPos, board);
                                    redPieces[tempPiece] = removePiece(redPieces[tempPiece], board); //remove the jumped piece
                                    redCount = getCount(redPieces); //recalculate the piece count
                                }
                                //check for king after every move
                                kingCheck(blackPieces, board);
                                //clearing console, redrawing the board, then resetting the cursor to 0,0
                                Console.Clear();
                                drawBoard(board, redCount, blackCount);
                                Console.SetCursorPosition(0,0);
                            }
                        }
                    }
                }
                //get the count of red and black pieces then clear console and redraw
                redCount = getCount(redPieces);
                blackCount = getCount(blackPieces);
                Console.Clear();
                drawBoard(board, redCount, blackCount);
            }//end game loop
            //clear console then check for which player has pieces left and display the winner
            Console.Clear();
            drawBoard(board, redCount, blackCount);
            if (redCount > 0){
                Console.SetCursorPosition(0,0); 
                Console.WriteLine("Red Wins!");
            }else {
                Console.SetCursorPosition(0,0); 
                Console.WriteLine("Black Wins!");
            }
            //pause console on press key message waiting to close the program
            pauseClose();
        }//end main

        //rules function that displays the rules of the game
        static void rules(){
            Console.WriteLine("Welcome to CheckerBot!\nThis game requires two people to play.");
            Console.WriteLine("The object is to eliminate all opposing checkers or to create a situation\n" +
            "in which it is impossible for your opponent to make any move.");
            Console.WriteLine("\nThis version has no flying king and forced captures are turned off.\n");
            Console.WriteLine("Player One is Red and Player Two is Black.");
            pause();
            Console.Clear();
        }

        //initializeBoard function that takes an empty 8 by 8 int array and initializes the values
        //this function creates the pattern of a checkerboard with 0 meaning a black square
        //and -1 meaning an unplayable red square
        static void initializeBoard(int[,] board){
            for (int i = 0; i < 8; i++){
                for (int j = 0; j < 8; j++){
                    if (i%2 == 0 && j%2 == 1){
                        board[i, j] = -1;
                    }else if (i%2 == 1 && j%2 == 0){
                        board[i, j] = -1;
                    }else{
                        board[i, j] = 0;
                    }
                }
            }
        }

        //verifyPiece function that accepts a piece array, an int array, and the 2d board array
        //the function finds the index of the piece in the array at the position passed as the int array position
        //the index of the piece is returned
        static int verifyPiece(piece[] array, int[] position, int[,] board){
            int temp = -1;
            if (position[0] == -1 || position[1] == -1){
                return temp;
            }
            for (int i = 0; i < array.Length; i++){
                if (array[i].row == position[0] && array[i].col == position[1]){
                    temp = i;
                }
            }
            return temp;
        }
        
        //getCount function that accepts a piece array and returns the count of pieces in play as an int
        static int getCount(piece[] array){
            int count = 0;
            for (int i = 0; i < array.Length; i++){
                if (array[i].inPlay){
                    count++;
                }
            }
            return count;
        }


        //startPieces function that accepts a piece array, a bool, and the 2d board array
        //this function initializes each of the pieces in the piece array and places a marker on the 2d board array
        //bool red determines where the pieces are set(at top or bottom of board)
        static void startPieces(piece[] array, bool red, int[,] board){
            int count = 0;
            for (int i = 0; i < 3; i++){
                for (int j = 0; j < 8; j++){
                    if (i % 2 == 0 && j % 2 == 0){
                        if (red){
                            array[count].red = true;
                            array[count].row =  i;
                            array[count].col =  j;
                            array[count].king = false;
                            array[count].inPlay = true;
                            board[i,j] = 1;
                            count++;
                            
                        }
                        
                    }else if(i % 2 == 1 && j % 2 == 1){
                        if (red){
                            array[count].red = true;
                            array[count].row =  i;
                            array[count].col =  j;
                            array[count].king = false;
                            array[count].inPlay = true;
                            board[i,j] = 1;
                            count++;
                        }
                    }else if(i % 2 == 0 && j % 2 == 1){
                        if (!red){
                            array[count].red = false;
                            array[count].row = 7 - i;
                            array[count].col =  j;
                            array[count].king = false;
                            array[count].inPlay = true;
                            board[(7-i),(j)] = 2;
                            count++;
                        }
                    }else if(i % 2 == 1 && j % 2 == 0){
                        if (!red){
                            array[count].red = false;
                            array[count].row = 7 - i;
                            array[count].col =  j;
                            array[count].king = false;
                            array[count].inPlay = true;
                            board[(7-i),j] = 2;
                            count++;
                        }
                    }
                }
            }
        }


        //movePiece function that accepts a piece, an int array, and the 2d board array
        //the function returns the same piece that is accepted with the row and col updated to the newPos
        //the board is then updated with the new piece position
        static piece movePiece(piece temp, int[] newPos, int[,] board){
            board[temp.row, temp.col] = 0;
            temp.col = newPos[1];
            temp.row = newPos[0];
            if (temp.king){    
                if (temp.red){
                    board[newPos[0], newPos[1]] = 3;
                }else {
                    board[newPos[0], newPos[1]] = 4;
                }
            }else{
                if (temp.red){
                    board[newPos[0], newPos[1]] = 1;
                }else {
                    board[newPos[0], newPos[1]] = 2;
                }
            }
            return temp;
        }

        //removePiece function that accepts a piece and the 2d board array
        //returns the same piece that was passed with the inplay bool deactivated
        //board is updated to reflect the change
        static piece removePiece(piece temp, int[,] board){
            temp.inPlay = false;
            board[temp.row, temp.col] = 0;
            return temp;
        }

        //kingCheck function that accepts a piece array and the 2d board array
        //function runs through the piece array to check for any pieces at the opposite end of the board
        //if a checker is at the opposite end of the board then the king bool is activated
        static void kingCheck(piece[] array, int[,] board){
            for (int i = 0; i < array.Length; i++){
                if (array[i].red){
                    if (array[i].row == 7){
                        array[i].king = true;
                        board[array[i].row, array[i].col] = 3;
                    }
                }else{
                    if (array[i].row == 0){
                        array[i].king = true;
                        board[array[i].row, array[i].col] = 4;
                    }
                }
            }
        }
        
        //Prompt function that accepts a string and returns the user's response to the string
        static string Prompt(string msg){
            Console.Write(msg + ": ");
            return Console.ReadLine();
        }

        //pause function that displays press key message then waits for a key press
        static void pause(){
            Console.WriteLine("Please press ANY key to continue.");
            Console.ReadKey();
        }

        //pauseTop function that displays the press key message at the top of the console then waits for a key press
        static void pauseTop(){
            Console.SetCursorPosition(0,0); 
            Console.WriteLine("Please press ANY key to continue.");
            Console.ReadKey();
        }

        //pauseClose function that dispalys a press key to close message then waits for key press
        static void pauseClose(){
            Console.SetCursorPosition(0,1); 
            Console.WriteLine("Please press ANY key to close the game.");
            Console.ReadKey();
        }

        //drawBoard function that accepts the 2d board array, an int, and an int
        //function uses a switch statement to read every element of the board array
        //based on the value in the array the switch statement will print the appropriate
        //color and character to the console
        static void drawBoard(int[,] board, int redCount, int blackCount){
            string block = "███";
            Console.SetCursorPosition(4, 4);
            Console.Write(" A  B  C  D  E  F  G  H ");
            for (int i = 0; i < 8; i++){
                Console.SetCursorPosition(2, (5 + i));//uses SetCursorPosition to regulate where on the console the board is printed
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(i + 1 + " ");
                for (int j = 0; j < 8; j++){
                    switch (board[i, j])
                    {
                        case -1: { Console.ForegroundColor = ConsoleColor.Red; Console.BackgroundColor = ConsoleColor.White; Console.Write(block); break;}
                        case 0: { Console.ForegroundColor = ConsoleColor.Black; Console.BackgroundColor = ConsoleColor.White; Console.Write(block); break;}
                        case 1: {Console.ForegroundColor = ConsoleColor.Red; Console.BackgroundColor = ConsoleColor.Black; Console.Write(" 1 "); break;}
                        case 2: {Console.ForegroundColor = ConsoleColor.Gray; Console.BackgroundColor = ConsoleColor.Black; Console.Write(" 2 "); break;}
                        case 3: {Console.ForegroundColor = ConsoleColor.Red; Console.BackgroundColor = ConsoleColor.Black; Console.Write(" K "); break;}
                        case 4: {Console.ForegroundColor = ConsoleColor.Gray; Console.BackgroundColor = ConsoleColor.Black; Console.Write(" K "); break;}
                        default:
                            break;
                    }
                }
            }
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine();
            Console.Write("     A  B  C  D  E  F  G  H \n\n");
            Console.WriteLine("  Red Count: {0}", redCount);
            Console.WriteLine("  Black Count: {0}", blackCount);
        }

        //getPosition function that accepts a string as user input
        //returns an int array that represents a cooresponding index
        //on the 2d board array
        //uses a switch case in order to allow ambiguity in the order of row/col entered first
        static int[] getPosition(string input){
            string temp = input.ToLower();
            int[] returnArray = new int[2];
            if (input.Length >= 2){
                if (temp[0] >= 49 && temp[0] <=57){
                    switch (temp[0]){
                        case '1': {returnArray[0] = 0;break;}
                        case '2': {returnArray[0] = 1;break;}
                        case '3': {returnArray[0] = 2;break;}
                        case '4': {returnArray[0] = 3;break;}
                        case '5': {returnArray[0] = 4;break;}
                        case '6': {returnArray[0] = 5;break;}
                        case '7': {returnArray[0] = 6;break;}
                        case '8': {returnArray[0] = 7;break;}
                        default: {returnArray[0] = -1; break;}
                    }
                    switch (temp[1])
                    {
                        case 'a': {returnArray[1] = 0;break;}
                        case 'b': {returnArray[1] = 1;break;}
                        case 'c': {returnArray[1] = 2;break;}
                        case 'd': {returnArray[1] = 3;break;}
                        case 'e': {returnArray[1] = 4;break;} 
                        case 'f': {returnArray[1] = 5;break;}
                        case 'g': {returnArray[1] = 6;break;}
                        case 'h': {returnArray[1] = 7;break;}
                        default: {returnArray[1] = -1; break;}
                    }

                }else if (temp[0] >= 97 && temp[0] <= 104){
                    switch (temp[0])
                    {
                        case 'a': {returnArray[1] = 0;break;}
                        case 'b': {returnArray[1] = 1;break;}
                        case 'c': {returnArray[1] = 2;break;}
                        case 'd': {returnArray[1] = 3;break;}
                        case 'e': {returnArray[1] = 4;break;}
                        case 'f': {returnArray[1] = 5;break;}
                        case 'g': {returnArray[1] = 6;break;}
                        case 'h': {returnArray[1] = 7;break;}
                        default: {returnArray[1] = -1; break;}
                    }
                    switch (temp[1]){
                        case '1': {returnArray[0] = 0;break;}
                        case '2': {returnArray[0] = 1;break;}
                        case '3': {returnArray[0] = 2;break;}
                        case '4': {returnArray[0] = 3;break;}
                        case '5': {returnArray[0] = 4;break;}
                        case '6': {returnArray[0] = 5;break;}
                        case '7': {returnArray[0] = 6;break;}
                        case '8': {returnArray[0] = 7;break;}
                        default: {returnArray[0] = -1; break;}
                    }
                }
            }
            return returnArray;

        }
        
    }
}
