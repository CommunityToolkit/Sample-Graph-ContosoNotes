import { useEffect, useState } from 'react';
import './App.css';
import { Login, MgtTemplateProps, Person } from '@microsoft/mgt-react';
import { Providers } from '@microsoft/mgt-element';
import { useIsSignedIn } from './mgt';

function App() {
  const [isSignedIn] = useIsSignedIn();

  let content;

  if (isSignedIn) {
    content = <Notes />;
  } else {
    content = <div>You need to be signed in to see notes!</div>
  }

  return (
    <div className="App">
      <header>
        <div className="top-nav">
          <div className="logo">
            <img src="/logo.png" alt="logo"></img>
          </div>
          <div className="sync-status">
            synced
          </div>
          <Login >
            <LoginTemplate template="signed-in-button-content" />
          </Login>
        </div>
      </header>
      <section className="content">
        {content}
      </section>
    </div>
  );
}

function LoginTemplate(props: MgtTemplateProps) {
  // return <Person personDetails={props.dataContext.personDetails} />;
  return <div>{props.dataContext.personDetails.givenName}</div>
}

function Notes() {

  const [isLoading, setIsLoading] = useState(true);

  const [note, setNote] = useState<any>(null);

  useEffect(() => {
    (async () => {
      const notesList = await getContentFromJsonFile('notesList');
      
      if (notesList && notesList.Items && notesList.Items.length) {
        const firstNoteFileName = notesList.Items[0].NotePageId;
        const noteContent = await getContentFromJsonFile(firstNoteFileName);
        setNote(noteContent);
        console.log(noteContent);
      }

      setIsLoading(false);
    })();
  }, []);

  let content;


  if (isLoading) {
    content = <div>Loading</div>
  } else if (note) {
    content = <Note noteItems={note.NoteItems} title={note.PageTitle} />;
  } else {
    content = <div>No notes</div>
  }

  return <div className="notes">
    {content}
  </div>;
}

function Note(props: any) {

  const {noteItems, title} = props;

  return <div className="notes-container">
    <h1 className="note-title">
      {title}
    </h1>
    {noteItems.map((note: any, index: number) => (
      <NoteItem key={index} note={note} />
    ))}
  </div>
}

function NoteItem(props: any) {
  const { note } = props;

  if (note.IsCompleted !== undefined) {
    return <TodoNoteItem note={note}/>
  }

  return <div className="notes-item" dangerouslySetInnerHTML={getHtmlFromText(note.Text)}></div>
}

function TodoNoteItem(props: any) {
  const {note} = props;

  const [state, setState] = useState(note);
  const [todoTask, setTodoTask] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (note.TodoTaskId && note.TodoTaskListId) {
      (async () => {
        const task = await getTask(note.TodoTaskId, note.TodoTaskListId);
        if (task) {
          setTodoTask(task);
          setState({...state, IsCompleted: task.status === 'completed', Text: task.title})
        }
        setLoading(false);
      })();
    } else {
      setLoading(false);
    }
  }, []);

  const onCompletedChanged = async (e: any) => {

    const isCompleted = state.IsCompleted;
    setState({...state, IsCompleted: !isCompleted});

    if (todoTask) {
      setLoading(true);
      const task = await Providers.client.api(`/me/todo/lists/${state.TodoTaskListId}/tasks/${state.TodoTaskId}`).patch({
        status: isCompleted ? 'notStarted' : 'completed'
      });
      console.log(task);
      setLoading(false);
    }
  }


  return <div className="notes-item task-item">
    <input disabled={loading} type="checkbox" checked={state.IsCompleted} onChange={onCompletedChanged} />
    <span>{state.Text}</span>
  </div>
}

const getContentFromJsonFile = async (fileName: string) => {
  try {
    let notesList = await Providers.client.api(`/me/drive/root:/Apps/ContosoNotes/${fileName}.json`).get();
    return await (await (await fetch(notesList['@microsoft.graph.downloadUrl']))).json();
  } catch (e) {
    return null;
  }
} 

const getTask = async (taskId: string, taskListId: string) => {
  try {
    let task = await Providers.client.api(`/me/todo/lists/${taskListId}/tasks/${taskId}`).get();
    return task;
  } catch (e) {
    return null;
  }
}

const getHtmlFromText = (text: string) => {
  return {
    __html: text.replaceAll('\r', '<br />')
  };
}

export default App;
