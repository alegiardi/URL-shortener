import { useEffect, useState } from "react";
import "./App.css";

function App() {
  const [originalUrl, setOriginalUrl] = useState("");
  const [shortenedUrls, setShortenedUrls] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(null);

  useEffect(() => {
    fetchUrls();
  }, []);

  const fetchUrls = async () => {
    try {
      setLoading(true);
      const response = await fetch("/api/url");
      if (!response.ok) {
        throw new Error("Failed to fetch URLs");
      }
      const data = await response.json();
      setShortenedUrls(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!originalUrl.trim()) {
      setError("Please enter a URL");
      return;
    }

    try {
      setLoading(true);
      setError(null);
      setSuccess(null);

      const response = await fetch("/api/url", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ originalUrl }),
      });

      if (!response.ok) {
        throw new Error("Failed to shorten URL");
      }

      setSuccess("URL shortened successfully.");
      setOriginalUrl("");
      fetchUrls();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (shortCode) => {
    try {
      setLoading(true);
      const response = await fetch(`/api/url/${shortCode}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        throw new Error("Failed to delete URL");
      }

      setSuccess("URL deleted successfully");
      fetchUrls();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdate = async (shortCode, newUrl) => {
    try {
      setLoading(true);
      const response = await fetch(`/api/url/${shortCode}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ newUrl }),
      });

      if (!response.ok) {
        throw new Error("Failed to update URL");
      }

      setSuccess("URL updated successfully");
      fetchUrls();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app-container">
      <h1>URL Shortener</h1>

      <section className="form-section">
        <form onSubmit={handleSubmit}>
          <div className="input-group">
            <input
              type="url"
              placeholder="Enter a URL to shorten"
              value={originalUrl}
              onChange={(e) => setOriginalUrl(e.target.value)}
              required
            />
            <button type="submit" disabled={loading}>
              {loading ? "Shortening..." : "Shorten URL"}
            </button>
          </div>
        </form>

        {error && <div className="error-message">{error}</div>}
        {success && <div className="success-message">{success}</div>}
      </section>

      <section className="urls-section">
        <h2>Your Shortened URLs</h2>
        {loading && <div className="loading">Loading...</div>}

        {shortenedUrls.length > 0 ? (
          <ul className="url-list">
            {shortenedUrls.map((url) => (
              <li key={url.id} className="url-item">
                 <div className="url-meta">
                  <span>
                  <strong>The new ✨shorter✨ link:</strong>{" "}
                  <a
                    href={`/api/url/${url.shortened}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    onClick={() => {
                      fetch(`/api/url/${url.shortened}/increment`, {
                        method: "POST",
                      }).then(() => {
                        fetchUrls();
                      });
                    }}
                  >
                    {window.location.origin}/api/url/{url.shortened}
                  </a>
                </span>
                <span className="click-count">(Clicks: {url.clickCount})</span>
              </div>
              <br />
              <div>
                <strong>Original:</strong> {url.originalUrl}
              </div>
                <div className="url-actions">
                  <button
                    onClick={() => {
                      const newUrl = prompt("Enter new URL: ", url.originalUrl);
                      if (newUrl && newUrl !== url.originalUrl) {
                        handleUpdate(url.shortened, newUrl);
                      }
                    }}
                  >
                    Edit
                  </button>
                  <button onClick={() => handleDelete(url.shortened)}>
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
        ) : (
          <p>No shortened URLs yet. Create one by entering a URL above.</p>
        )}
      </section>
    </div>
  );
}

export default App;
