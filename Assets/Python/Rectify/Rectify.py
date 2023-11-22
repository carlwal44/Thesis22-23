import numpy as np
import cv2
import matplotlib.pyplot as plt
import os.path
import json
from pathlib import Path
import random

def rectify():
    # INPUT - IMAGE
    path = str(Path(__file__).parent) + r"\facade.png"

    if((os.path.exists(path)) == False):
        print("file not found: " + path)

    img = cv2.imread(path,cv2.COLOR_BGR2RGB)
    img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

    # INPUT - JSON
    pathJSON = str(Path(__file__).parent) + r"\JSON.txt"
    f = open(pathJSON,)
    clickedPoints = json.load(f)
    f.close()

    # Rectification
    #rows,cols,ch = img.shape
    pts1 = np.float32([[clickedPoints['point1']['x'],640-clickedPoints['point1']['y']],
                    [clickedPoints['point2']['x'],640-clickedPoints['point2']['y']],
                    [clickedPoints['point3']['x'],640-clickedPoints['point3']['y']],
                    [clickedPoints['point4']['x'],640-clickedPoints['point4']['y']]])
    
    #sorting all cornors from left up, right up, right down to left down
    sorted_indices = np.argsort(pts1[:, 1])
    sorted_pts1 = pts1[sorted_indices]
    if(sorted_pts1[0,0]> sorted_pts1[1,0]):
        sorted_pts1[[0,1]] = sorted_pts1[[1,0]]

    if(sorted_pts1[2,0]< sorted_pts1[3,0]):
        sorted_pts1[[2,3]] = sorted_pts1[[3,2]]

    #get the average width and height that was selected to use as dimensions of resulting image
    mean_x = int(round(((sorted_pts1[1,0]+sorted_pts1[2,0])-(sorted_pts1[0,0]+sorted_pts1[3,0]))/2, 0))
    mean_y = int(round(((sorted_pts1[3,1]+sorted_pts1[2,1])-(sorted_pts1[0,1]+sorted_pts1[1,1]))/2, 0))
    
    pts2 = np.float32([[0,0],[mean_x,0],[mean_x,mean_y],[0,mean_y]])

    M = cv2.getPerspectiveTransform(sorted_pts1,pts2)

    dst = cv2.warpPerspective(img,M,(mean_x,mean_y))
    dst = cv2.cvtColor(dst, cv2.COLOR_RGB2BGR)

    #save in the map
    #index = random.randint(0,100000)
    #print(index)
    #cv2.imwrite('result_rectify'+str(index)+'.jpg', dst)
    os.chdir(str(Path(__file__).parent.parent) + r"\win_prediction\result")
    cv2.imwrite('result_rectify.jpg', dst)

if __name__ == "__main__":
    rectify()

