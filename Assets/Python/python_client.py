from win_prediction.infer import infer
from Rectify.Rectify import rectify
from pathlib import Path
import time



def main():
    #**************************************************************************************************
    # Rectify
    #**************************************************************************************************
    path = str(Path(__file__).parent.parent) + r"\Resources\Progress"
    f = open(path + r"\pythonStarted.txt","w")
    f.flush()
    f.close()

    beginT = time.time()

    rectify()

    endt = time.time() - beginT
    f = open(path + r"\rectify.txt","w")
    f.write(str(endt))
    f.close()
    #**************************************************************************************************
    # Infer
    #**************************************************************************************************
    beginT = time.time()

    infer()

    endt = time.time() - beginT
    f = open(path + r"\Machine Learning.txt","w")
    f.write(str(endt))
    f.close()
    
if __name__ == "__main__":
    main()
