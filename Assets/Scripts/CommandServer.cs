using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SocketIO;
using UnityEngine;
using UnityEngine.UI;

public class CommandServer : MonoBehaviour
{
    public RobotRemoteControl robotRemoteControl;
    public IRobotController robotController;
    public Camera frontFacingCamera;
    public RobotArmActuator armActuator;
    private SocketIOComponent _socket;
    public RawImage inset1;
    public RawImage inset2;
    public RawImage inset3;

//	Texture2D inset1Tex;
//	Texture2D inset2Tex;
//	Texture2D inset3Tex;

    void Start()
    {
        _socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
        // _socket.On("open", OnOpen);
        _socket.On("connect", OnConnect);
        _socket.On("data", OnData);
        _socket.On("arm_command", OnArmCommand);
        _socket.On("manual", OnManual);
        _socket.On("fixed_turn", OnFixedTurn);
        _socket.On("pickup", OnPickup);
//		_socket.On ( "get_samples", GetSamplePositions );
        robotController = robotRemoteControl.robot;
        frontFacingCamera = robotController.recordingCam;
//		inset1Tex = new Texture2D ( 1, 1 );
//		inset2Tex = new Texture2D ( 1, 1 );
//		inset3Tex = new Texture2D ( 1, 1 );
        
    }

    private void OnConnect(SocketIOEvent obj)
    {
        Debug.Log("On Connect to Server");
        EmitTelemetry();
        EmitArmTelemetry();
    }

    private void OnOpen(SocketIOEvent obj)
    {
//		Debug.Log("Connection Open");
        EmitTelemetry();
    }

    // 
    private void OnManual(SocketIOEvent obj)
    {
        EmitTelemetry();
    }

    private void OnData(SocketIOEvent obj)
    {
//		Debug.Log ( "Steer" );
        JSONObject jsonObject = obj.data;
        robotRemoteControl.SteeringAngle =
            -float.Parse(jsonObject.GetField("steering_angle").str); // he wanted the angles CCW
        robotRemoteControl.ThrottleInput = float.Parse(jsonObject.GetField("throttle").str);
        if (jsonObject.HasField("brake"))
            robotRemoteControl.BrakeInput = float.Parse(jsonObject.GetField("brake").str);
        else
            robotRemoteControl.BrakeInput = 0;

        // testing memory leak
//		return;

        // try to load image1
        var loaded = false;
        Vector2 size;
        Texture2D tex = null;
        string imageInfo = "";
        byte[] imageBytes = null;
        if (jsonObject.HasField("inset_image1"))
            imageInfo = jsonObject.GetField("inset_image1").str;
        if (!string.IsNullOrEmpty(imageInfo))
            imageBytes = Convert.FromBase64String(imageInfo);
        if (imageBytes != null && imageBytes.Length != 0)
        {
            tex = new Texture2D(1, 1);
            loaded = tex.LoadImage(imageBytes, true);
        }

        if (loaded && inset1 != null && tex.width > 1 && tex.height > 1)
        {
            if (inset1.texture != null)
                Destroy(inset1.texture);
            inset1.texture = tex;
//			inset1Tex = tex;
//			inset1.texture = inset1Tex;
            size = inset1.rectTransform.sizeDelta;
            size.x = 1f * tex.width / tex.height * size.y;
//			size.x = 1f * inset1Tex.width / inset1Tex.height * size.y;
            inset1.rectTransform.sizeDelta = size;
            inset1.CrossFadeAlpha(1, 0.3f, false);
        }
        else if (inset1 != null)
        {
//			tex = null;
        }

        // try to load image2
        loaded = false;

        tex = null;
        imageInfo = "";
        imageBytes = null;
        if (jsonObject.HasField("inset_image2"))
            imageInfo = jsonObject.GetField("inset_image2").str;
        if (!string.IsNullOrEmpty(imageInfo))
            imageBytes = Convert.FromBase64String(imageInfo);
        if (imageBytes != null && imageBytes.Length != 0)
        {
            tex = new Texture2D(1, 1);
            loaded = tex.LoadImage(imageBytes, true);
        }

        if (loaded && inset2 != null && tex.width > 1 && tex.height > 1)
        {
            if (inset2.texture != null)
                Destroy(inset2.texture);
            inset2.texture = tex;
//			inset2Tex = tex;
//			inset2.texture = inset2Tex;
            size = inset2.rectTransform.sizeDelta;
            size.x = 1f * tex.width / tex.height * size.y;
//			size.x = 1f * inset2Tex.width / inset2Tex.height * size.y;
            inset2.rectTransform.sizeDelta = size;
            inset2.CrossFadeAlpha(1, 0.3f, false);
        }
        else if (inset2 != null)
        {
//			if ( inset1.texture != null )
//				Destroy ( inset1.texture );
        }

        // try to load image3
        loaded = false;
//		if ( tex != null )
//			Destroy ( tex );
        tex = null;
        imageInfo = "";
        imageBytes = null;
        if (jsonObject.HasField("inset_image3"))
            imageInfo = jsonObject.GetField("inset_image3").str;
        if (!string.IsNullOrEmpty(imageInfo))
            imageBytes = Convert.FromBase64String(imageInfo);
        if (imageBytes != null && imageBytes.Length != 0)
        {
            tex = new Texture2D(1, 1);
            loaded = tex.LoadImage(imageBytes, true);
        }

        if (loaded && inset3 != null && tex.width > 1 && tex.height > 1)
        {
            if (inset3.texture != null)
                Destroy(inset3.texture);
            inset3.texture = tex;
//			inset3Tex = tex;
//			inset3.texture = inset3Tex;
            size = inset3.rectTransform.sizeDelta;
            size.x = 1f * tex.width / tex.height * size.y;
//			size.x = 1f * inset3Tex.width / inset3Tex.height * size.y;
            inset3.rectTransform.sizeDelta = size;
            inset3.CrossFadeAlpha(1, 0.3f, false);
        }
        else if (inset3 != null)
        {
        }

//		if ( tex != null )
//			Destroy ( tex );
        tex = null;
        EmitTelemetry();
    }
    
    private void OnArmCommand(SocketIOEvent obj)
    {
        if (armActuator == null)
        {
            Debug.LogWarning("⚠️ armActuator not assigned - ignoring arm command");
            return;
        }

        Debug.Log($"🤖 ARM COMMAND: {obj.data}");
        JSONObject armData = obj.data;
        
        
        // IMPORTANT: Ensure arm stays enabled and unfolded
        Debug.Log($"Actuator state: {armActuator.enabled}");
        if (!armActuator.enabled)
        {
            armActuator.enabled = true;
            Debug.Log("🔄 Re-enabling arm actuator");
        }
        
        if (armData.HasField("command"))
        {
            JSONObject command = armData.GetField("command");
            string commandType = command.HasField("type") ? command.GetField("type").str : "";
            
            Debug.Log($"🎯 Arm command type: {commandType}");
            
            switch (commandType)
            {
                case "CartesianMove":
                    HandleArmCartesianMove(command);
                    break;
                case "Home":
                    HandleArmHome();
                    break;
                case "Stop":
                    HandleArmStop();
                    break;
                default:
                    Debug.LogWarning($"❓ Unknown arm command: {commandType}");
                    break;
            }
        }
    }
    
    private void HandleArmCartesianMove(JSONObject command)
    {
        if (armActuator == null || armActuator.wrist == null)
        {
            Debug.LogError("❌ Arm components not configured");
            return;
        }

        // Get movement values
        float x = command.HasField("x") ? command.GetField("x").f : 0f;
        float y = command.HasField("y") ? command.GetField("y").f : 0f;
        float z = command.HasField("z") ? command.GetField("z").f : 0f;
        
        Debug.Log($"🎯 Move: x={x:F3}, y={y:F3}, z={z:F3}");
        
        // Force unfold to ensure arm is ready (rover movement might fold it)
        armActuator.Unfold(false);
        
        // Scale movement for visibility (increase scale)
        float scale = 1.2f; // Larger movements
        Vector3 scaledMove = new Vector3(x, y, z) * scale;
        
        // Convert coordinates: Rust(x,y,z) -> Unity(y,z,x)
        Vector3 currentPos = armActuator.wrist.position;
        Vector3 deltaMove = new Vector3(scaledMove.y, scaledMove.z, scaledMove.x);
        Vector3 targetPos = currentPos + deltaMove;
        
        Debug.Log($"📍 Current: {currentPos}");
        Debug.Log($"📏 Delta: {deltaMove}");
        Debug.Log($"🎯 Target: {targetPos}");
        
        try
        {
            // Use target index 1 for extended position
            armActuator.SetTarget(1, targetPos);
            
            float speed = command.HasField("max_velocity") ? command.GetField("max_velocity").f : 1.5f;
            armActuator.MoveToTarget(speed, false);
            
            Debug.Log($"✅ Arm moving with speed {speed}");
            
            // Start monitoring to ensure movement happens
            StartCoroutine(CheckArmMovement(currentPos, targetPos));
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Arm move failed: {e.Message}");
        }
    }

    private void HandleArmHome()
    {
        if (armActuator == null) return;
        
        Debug.Log("🏠 ARM HOME");
        try
        {
            armActuator.enabled = true;  // Ensure enabled
            armActuator.Fold(true);
            Debug.Log("✅ Arm homing");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Home failed: {e.Message}");
        }
    }

    private void HandleArmStop()
    {
        if (armActuator == null) return;
        
        Debug.Log("🛑 ARM STOP");
        try
        {
            armActuator.StopAllCoroutines();
            if (armActuator.wrist != null)
            {
                Vector3 currentPos = armActuator.wrist.position;
                armActuator.SetTarget(1, currentPos);
            }
            Debug.Log("✅ Arm stopped");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Stop failed: {e.Message}");
        }
    }

    private IEnumerator CheckArmMovement(Vector3 startPos, Vector3 targetPos)
    {
        float timeout = 5.0f;
        float startTime = Time.time;
        
        while (Time.time - startTime < timeout)
        {
            if (armActuator == null || armActuator.wrist == null) yield break;
            
            Vector3 currentPos = armActuator.wrist.position;
            float moved = Vector3.Distance(currentPos, startPos);
            float remaining = Vector3.Distance(currentPos, targetPos);
            
            Debug.Log($"📊 Moved: {moved:F3}m, Remaining: {remaining:F3}m");
            
            if (remaining < 0.05f)
            {
                Debug.Log("✅ Arm reached target!");
                yield break;
            }
            
            yield return new WaitForSeconds(1.0f);
        }
        
        Debug.LogWarning("⏰ Arm movement timeout");
    }

    private void EmitArmTelemetry()
    {
        if (armActuator == null || armActuator.wrist == null) return;
        
        try
        {
            JSONObject armJson = new JSONObject();
            
            Vector3 wristPos = armActuator.wrist.position;
            
            // End effector pose
            JSONObject poseArray = new JSONObject();
            poseArray.Add(wristPos.z);  // Unity Z -> Rust X
            poseArray.Add(wristPos.x);  // Unity X -> Rust Y
            poseArray.Add(wristPos.y);  // Unity Y -> Rust Z
            poseArray.Add(0f); poseArray.Add(0f); poseArray.Add(0f); // rotation
            
            armJson.AddField("end_effector_pose", poseArray);
            armJson.AddField("is_moving", armActuator.enabled);
            armJson.AddField("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            
            _socket.Emit("arm_telemetry", armJson);
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Arm telemetry failed: {e.Message}");
        }
    }

    void OnFixedTurn(SocketIOEvent obj)
    {
        JSONObject json = obj.data;
        float angle = float.Parse(json.GetField("angle").str);
        float time = 0;
        if (json.HasField("time"))
            time = float.Parse(json.GetField("time").str);
        robotRemoteControl.FixedTurn(angle, time);
        EmitTelemetry();
    }

    void OnPickup(SocketIOEvent obj)
    {
        robotRemoteControl.PickupSample();
        EmitTelemetry();
    }

    void EmitTelemetry()
    {
//		Debug.Log ( "Emitting" );
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            print("Attempting to Send...");

            // Collect Data from the Car
            Dictionary<string, string> data = new Dictionary<string, string>();

            data["steering_angle"] = robotController.SteerAngle.ToString("N4");
//			data["vert_angle"] = robotController.VerticalAngle.ToString ("N4");
            data["throttle"] = robotController.ThrottleInput.ToString("N4");
            data["brake"] = robotController.BrakeInput.ToString("N4");
            data["speed"] = robotController.Speed.ToString("N4");
            Vector3 pos = robotController.Position;
            data["position"] = pos.x.ToString("N4") + robotController.csvSeparatorChar + pos.z.ToString("N4");
            data["pitch"] = robotController.Pitch.ToString("N4");
            // new: convert the angle to CCW, x-based
            data["yaw"] = IRobotController.ConvertAngleToCCWXBased(robotController.Yaw).ToString("N4");
            data["roll"] = robotController.Roll.ToString("N4");
//			data["fixed_turn"] = robotController.IsTurningInPlace ? "1" : "0";
            data["near_sample"] = robotController.IsNearObjective ? "1" : "0";
            data["picking_up"] = robotController.IsPickingUpSample ? "1" : "0";
//			Debug.Log ("picking_up is " + robotController.IsPickingUpSample);
            data["sample_count"] = ObjectiveSpawner.samples.Length.ToString();

            StringBuilder sample_x = new StringBuilder();
            StringBuilder sample_y = new StringBuilder();
            for (int i = 0; i < ObjectiveSpawner.samples.Length; i++)
            {
                GameObject go = ObjectiveSpawner.samples[i];
                sample_x.Append(go.transform.position.x.ToString("N2") + robotController.csvSeparatorChar);
                sample_y.Append(go.transform.position.z.ToString("N2") + robotController.csvSeparatorChar);
            }

            if (ObjectiveSpawner.samples.Length != 0)
            {
                sample_x.Remove(sample_x.Length - 1, 1);
                sample_y.Remove(sample_y.Length - 1, 1);
            }

            data["samples_x"] = sample_x.ToString();
            data["samples_y"] = sample_y.ToString();
            data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(frontFacingCamera));

//			Debug.Log ("sangle " + data["steering_angle"] + " vert " + data["vert_angle"] + " throt " + data["throttle"] + " speed " + data["speed"] + " image " + data["image"]);
            _socket.Emit("telemetry", new JSONObject(data));
            
            // Add arm telemetry
            EmitArmTelemetry();
        });
    }
}