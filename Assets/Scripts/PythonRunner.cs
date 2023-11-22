using System.Diagnostics;
using System.IO;
using System;
using UnityEngine;

public class PythonRunner : MonoBehaviour
{
    public Process process;
    [HideInInspector]
    public bool hasStarted;
    GameObject loading;

    void Start()
    {
        hasStarted = false;
    }

    void Update()
    {
        if(hasStarted)
        {
            if(process.HasExited)
            {
                print("Python code has exited");
                hasStarted = false;
                var generateClickable = GameObject.Find("/API Object/Canvas/Progress/Generate");
                generateClickable.SetActive(true);
            }
        }
    }

    public void runML()
    {
        process = runPython(process, hasStarted);
        hasStarted = true;
    }

    static Process runPython(Process process, bool started)
    {
        // ...
        UnityEngine.Debug.Log("python file is running");
        var select = GameObject.Find("/API Object/Canvas/SelectPoints");
        select.SetActive(false);
        
        // Replace "python.exe" with the path to your Python interpreter
        string pythonPath = @"C:\\Users\\carlw\\AppData\\Local\\Programs\\Python\\Python310\python.exe";
        string scriptPath = Application.dataPath + @"\Python\python_client.py";

        ProcessStartInfo startInfo = new ProcessStartInfo(pythonPath);
        startInfo.Arguments = scriptPath;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = true;        // changed to suppress cmd window popup
        startInfo.WindowStyle = ProcessWindowStyle.Normal;

        process = new Process();
        process.StartInfo = startInfo;
        process.OutputDataReceived += Process_OutputDataReceived;

        process.Start();
        return process;

        //process.BeginOutputReadLine();

        //StreamReader reader = process.StandardOutput;
        //string output = reader.ReadToEnd();
        //process.WaitForExit();
        
    }

    static bool checkIfExited(Process process)
    {
        return(process.HasExited);
    }
    static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        // asynchronously print the output to the console
        //Console.WriteLine(e.Data);
        UnityEngine.Debug.Log(e.Data);
    }
}